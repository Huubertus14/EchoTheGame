using Fusion;
using Project.Echo.Caches;
using Project.Echo.Player.Visuals;
using Project.Echo.Projectiles.Behaviours;
using Project.Echo.Projectiles.Contexts;
using Project.Echo.Projectiles.Structs;
using Project.Echo.Weapons;
using System;
using UnityEngine;

namespace Project.Echo.Projectiles
{
	public class ProjectileManager : NetworkBehaviour
	{
		[SerializeField]
		private bool _fullProxyPrediction = false;
		[SerializeField]
		private Projectile[] _projectilePrefabs;
		[SerializeField] private BasicCannonBehaviour _cannon;

		[Networked, Capacity(96)]
		private NetworkArray<ProjectileData> _projectiles { get; }
		[Networked]
		[SerializeField] private int _projectileCount { get; set; }
		
		[SerializeField] private Projectile[] _visibleProjectiles = new Projectile[128];
		[SerializeField] private int _visibleProjectileCount;
		[SerializeField] private ProjectileContext _projectileContext;
		[SerializeField] private RawInterpolator _projectilesInterpolator;
		private ObjectCache _objectCache;
		[SerializeField]private PlayerVisualController _visualController;

		private void Awake()
		{
			_visualController = GetComponentInChildren<PlayerVisualController>();
			_visualController.ColorSet += OnColorSet;
		}

		public override void Spawned()
		{
			base.Spawned();
			_objectCache = GameReferenceManager.GetObjectCache;
			_cannon = GetComponent<BasicCannonBehaviour>();
			OnSpawned();
		}

		public void AddProjectile(Projectile projectilePrefab, byte weaponAction = 0)
		{
			var fireData = projectilePrefab.GetFireData(Runner, _cannon.GetBulletTransform.position, _cannon.GetBulletTransform.forward);
			AddProjectile(projectilePrefab, fireData, weaponAction);
		}

		public void AddProjectile(Projectile projectilePrefab, ProjectileData data, byte weaponAction = 0)
		{
			int prefabIndex = _projectilePrefabs.IndexOf(projectilePrefab);

			if (prefabIndex < 0)
			{
				Debug.LogError($"Projectile {projectilePrefab} not found. Add it in ProjectileManager prefab array.");
				return;
			}

			data.PrefabId = (byte)prefabIndex;
			data.FireTick = Runner.Tick;
			data.IsActive = true;
			data.WeaponAction = weaponAction;

			int projectileIndex = _projectileCount % _projectiles.Length;

			var previousData = _projectiles[projectileIndex];
			if (previousData.IsActive == true && previousData.IsFinished == false)
			{
				Debug.LogError("No space for another projectile - projectile buffer should be increased or projectile lives too long");
			}

			_projectiles.Set(projectileIndex, data);

			_projectileCount++;
		}

		public void OnSpawned()
		{
			_visibleProjectileCount = _projectileCount;

			_projectileContext = new ProjectileContext()
			{
				Runner = Runner,
				Cache = _objectCache,
				InputAuthority = Object.InputAuthority,
				BulletBegin = _cannon.GetBulletTransform,
				OwnerObjectInstanceID = gameObject.GetInstanceID(),
				ProjectileColor = _visualController.PlayerColor
			};

			_projectilesInterpolator = GetInterpolator(nameof(_projectiles));
		}

		private void OnColorSet(Color col)
		{
			_visualController.ColorSet -= OnColorSet;
			_projectileContext.ProjectileColor = col;
			Debug.Log($"Color set {col}");
		}

		public override void FixedUpdateNetwork()
		{
			base.FixedUpdateNetwork();// Projectile calculations are processed only on input or state authority
									  // unless full proxy prediction is turned on
			if (IsProxy == true && _fullProxyPrediction == false)
				return;

			_projectileContext.FloatTick = Runner.Tick;

			for (int i = 0; i < _projectiles.Length; i++)
			{
				var projectileData = _projectiles[i];

				if (projectileData.IsActive == false)
					continue;
				if (projectileData.IsFinished == true)
					continue;


				var prefab = _projectilePrefabs[projectileData.PrefabId];
				prefab.OnFixedUpdate(_projectileContext, ref projectileData);

				_projectiles.Set(i, projectileData);
			}
		}

		public void OnRender( )
		{
			// Visuals are not spawned on dedicated server at all
			if (Runner.Mode == SimulationModes.Server)
				return;

			RenderProjectiles();
		}
		private void RenderProjectiles()
		{
			_projectilesInterpolator.TryGetArray(_projectiles, out var fromProjectiles, out var toProjectiles, out float interpolationAlpha);

			var simulation = Runner.Simulation;
			bool interpolate = IsProxy == true && _fullProxyPrediction == false;

			if (interpolate == true)
			{
				// For proxies we move projectile in snapshot interpolated time
				_projectileContext.FloatTick = simulation.InterpFrom.Tick + (simulation.InterpTo.Tick - simulation.InterpFrom.Tick) * simulation.InterpAlpha;
			}
			else
			{
				_projectileContext.FloatTick = simulation.Tick + simulation.StateAlpha;
			}

			int bufferLength = _projectiles.Length;

			// Our predicted projectiles were not confirmed by the server, let's discard them
			for (int i = _projectileCount; i < _visibleProjectileCount; i++)
			{
				var projectile = _visibleProjectiles[i % bufferLength];
				if (projectile != null)
				{
					// We are not destroying projectile immediately,
					// projectile can decide itself how to react
					projectile.Discard();
				}
			}

			int minFireTick = Runner.Tick - (int)(Runner.Simulation.Config.TickRate * 0.5f);

			// Let's spawn missing projectile gameobjects
			for (int i = _visibleProjectileCount; i < _projectileCount; i++)
			{
				int index = i % bufferLength;
				var projectileData = _projectiles[index];

				// Projectile is long time finished, do not spawn visuals for it
				// Note: We cannot check just IsFinished, because some projectiles are finished
				// immediately in one tick but the visuals could be longer running
				if (projectileData.IsFinished == true && projectileData.FireTick < minFireTick)
					continue;

				if (_visibleProjectiles[index] != null)
				{
					Debug.LogError("No space for another projectile gameobject - projectile buffer should be increased or projectile lives too long");
					DestroyProjectile(_visibleProjectiles[index]);
				}

				byte weaponAction = projectileData.WeaponAction;
				

				_visibleProjectiles[index] = CreateProjectile(_projectileContext, ref projectileData);
			}

			// Update all visible projectiles
			for (int i = 0; i < bufferLength; i++)
			{
				var projectile = _visibleProjectiles[i];
				if (projectile == null)
					continue;

				if (projectile.IsDiscarded == false)
				{
					var data = _projectiles[i];

					if (data.PrefabId != projectile.PrefabId)
					{
						Debug.LogError($"{Runner.name}: Incorrect spawned prefab. Should be {data.PrefabId}, actual {projectile.PrefabId}. This should not happen.");
						DestroyProjectile(projectile);
						_visibleProjectiles[i] = null;
						continue;
					}

					bool interpolateProjectile = interpolate == true && projectile.NeedsInterpolationData;

					// Prepare interpolation data if needed
					ProjectileInterpolationData interpolationData = default;
					if (interpolateProjectile == true)
					{
						interpolationData.From = fromProjectiles.Get(i);
						interpolationData.To = toProjectiles.Get(i);
						interpolationData.Alpha = interpolationAlpha;
					}

					_projectileContext.Interpolate = interpolateProjectile;
					_projectileContext.InterpolationData = interpolationData;

					// If barrel transform is not available anymore (e.g. weapon was switched before projectile finished)
					// let's use at least some dummy (first) one. Doesn't matter at this point much.
					//_projectileContext.BulletBegin = weaponBarrelTransforms[barrelTransformIndex]; //TOdo set on place

					projectile.OnRender(_projectileContext, ref data);
				}

				if (projectile.IsFinished == true)
				{
					DestroyProjectile(projectile);
					_visibleProjectiles[i] = null;
				}
			}

			_visibleProjectileCount = _projectileCount;
		}

		private Projectile CreateProjectile(ProjectileContext context, ref ProjectileData data)
		{
			var projectile = _objectCache.Get(_projectilePrefabs[data.PrefabId]);

			Runner.MoveToRunnerScene(projectile);

			projectile.Activate(context, _visualController.PlayerColor, ref data);

			return projectile;
		}

		private void DestroyProjectile(Projectile projectile)
		{
			projectile.Deactivate(_projectileContext);

			_objectCache.Return(projectile.gameObject);
		}

		private void LogMessage(string message)
		{
			Debug.Log($"{Runner.name} (tick: {Runner.Tick}, frame: {Time.frameCount}) - {message}");
		}
	}
}
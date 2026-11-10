using UnityEngine;
using Fusion;
using System;
using Project.Echo.Menu;

namespace Project.Echo.Player.Visuals
{
    public class PlayerVisualController : NetworkBehaviour 
    {
		private Renderer[] _renderers;
		private PlayerHealthController _health;
		private float _revealedUntil;
		[Header("Player colors")]
		[SerializeField] private EchoMenuTheme _theme;
		private Color _playerColor;

		public static Color DefaultPlayerColor => new Color32(155, 233, 226, 255);

		public Color PlayerColor
		{
			get => _playerColor;
			set
			{
				_playerColor = value;
				if (_renderers == null) return;
				SetColor(value);
				ColorSet?.Invoke(value);
			}
		}

		public Action<Color> ColorSet { get; set; }

		public override void Spawned()
		{
			_renderers = GetComponentsInChildren<Renderer>();
			_health = GetComponentInParent<PlayerHealthController>();
			_revealedUntil = 0f;

			var tuning = GetComponentInParent<PlayerNetworkedController>().Tuning;
			Color color = Object.HasInputAuthority ? (_theme ? _theme.seafoam : DefaultPlayerColor) : tuning.opponentColor;
			foreach (var entry in tuning.playerColorOverrides)
				if (entry.playerId == Object.InputAuthority.PlayerId)
				{
					color = entry.color;
					break;
				}
			PlayerColor = color;
		}

		public void RevealContact(float duration) => _revealedUntil = Mathf.Max(_revealedUntil, Time.time + duration);

		private void OnDisable() => _revealedUntil = 0f;

		private void LateUpdate()
		{
			if (!Object || !Object.IsValid || _renderers == null || !_health) return;
			var viewer = PlayerNetworkedController.LocalPlayer;
			if (NetworkedPlayerGameModeController.Coordinator?.Phase != 2) _revealedUntil = 0f;
			bool visible = _health.Alive && (HasInputAuthority || !viewer || viewer.Runner != Runner ||
				Time.time < _revealedUntil ||
				SonarPropagation.HasClearPath(Runner.GetPhysicsScene(), viewer.transform.position, _health.transform.position));
			foreach (var renderer in _renderers)
				if (renderer) renderer.forceRenderingOff = !visible;
			_health.SetContactVisible(visible);
		}

		private void SetColor(Color col)
		{
			foreach (var ren in _renderers)
			{
				ren.material.color = col;
			}
		}
	}
}

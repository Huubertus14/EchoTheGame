using Fusion;
using Project.Echo.Extensions;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Project.Echo.Projectiles.Structs
{
	[StructLayout(LayoutKind.Explicit)]
	public struct ProjectileData : INetworkStruct
	{
		public bool IsActive { get { return _state.IsBitSet(0); } set { _state.SetBit(0, value); } }
		public bool IsFinished { get { return _state.IsBitSet(1); } set { _state.SetBit(1, value); } }

		[FieldOffset(0)]
		private byte _state;

		[FieldOffset(1)]
		public byte PrefabId;
		[FieldOffset(2)]
		public byte WeaponAction;
		[FieldOffset(3)]
		public int FireTick;
		[FieldOffset(7)]
		public Vector3 FirePosition;
		[FieldOffset(19)]
		public Vector3 FireVelocity;
		[FieldOffset(31)]
		public Vector3 ImpactPosition;
		[FieldOffset(43)]
		public Vector3 ImpactNormal;

		// Custom projectile data
		[FieldOffset(55)]
		public KinematicData Kinematic;
	}

	public struct ProjectileInterpolationData
	{
		public ProjectileData From;
		public ProjectileData To;
		public float Alpha;
	}
	public struct KinematicData : INetworkStruct
	{
		public NetworkBool HasStopped;
		public Vector3 FinishedPosition;
		public int StartTick;
		public byte BounceCount;
	}
}
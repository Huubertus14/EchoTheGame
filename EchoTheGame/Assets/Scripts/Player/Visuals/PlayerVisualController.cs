using UnityEngine;
using Fusion;
using System;

namespace Project.Echo.Player.Visuals
{
    public class PlayerVisualController : NetworkBehaviour 
    {
		[Serializable]
		private struct PlayerColorOverride
		{
			public int playerId;
			[ColorUsage(false)] public Color color;
		}

		private Renderer[] _renderers;
		[Header("Player colors")]
		[SerializeField, ColorUsage(false)] private Color _ownPlayerColor = Color.blue;
		[SerializeField, ColorUsage(false)] private Color _otherPlayerColor = Color.red;
		[Tooltip("Optional colors by Fusion player ID, shared by submarine, projectiles and sonar. Set on the player prefab before starting a match.")]
		[SerializeField] private PlayerColorOverride[] _playerColorOverrides = Array.Empty<PlayerColorOverride>();
		private Color _playerColor;

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

			Color color = Object.HasInputAuthority ? _ownPlayerColor : _otherPlayerColor;
			foreach (var entry in _playerColorOverrides)
				if (entry.playerId == Object.InputAuthority.PlayerId)
				{
					color = entry.color;
					break;
				}
			PlayerColor = color;
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

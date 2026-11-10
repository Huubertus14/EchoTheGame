# Echo project conventions

For UI work, read `docs/UI_STYLE.md` and reuse `EchoTheGame/Assets/UI/Echo/EchoMenuTheme.asset`.
The approved direction is a top-down sonar world, midnight navy and luminous seafoam cyan.
Keep background artwork independent of real, editable Unity UI; icons must be separate image assets.
Do not bake labels or buttons into backgrounds. Preserve keyboard/controller navigation.

Put new or changed gameplay tuning in a typed ScriptableObject under
`EchoTheGame/Assets/Settings`, grouped by the object or game mode that owns it.
Use the asset as the single source for balance values, limits, timings and
multipliers. Prefabs and scenes should hold the asset reference and structural
wiring. Keep runtime state separate from the asset so configuration can later
come from a cloud or database source.
Give each gameplay tuning field an Inspector tooltip with its effect, units and
any special values.

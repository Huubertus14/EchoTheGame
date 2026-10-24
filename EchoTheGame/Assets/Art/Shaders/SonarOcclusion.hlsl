#ifndef ECHO_SONAR_OCCLUSION
#define ECHO_SONAR_OCCLUSION
int _EchoSonarCount;
int _EchoOcclusionEnabled;
float4 _EchoSonarOrigins[32];
float4 _EchoSonarWaves[32];
float4 _EchoSonarColors[32];
float4 _EchoSonarSectors[32]; // reflected direction XZ, cosine of half-angle
TEXTURE2D(_EchoSonarOcclusion);
SAMPLER(sampler_EchoSonarOcclusion);

float EchoVisible(float3 world, int pulse, float objectId)
{
    if (_EchoOcclusionEnabled == 0) return 1;
    float2 delta = world.xz - _EchoSonarOrigins[pulse].xy;
    float distanceFromSource = length(delta);
    float4 sector = _EchoSonarSectors[pulse];
    if (sector.z > -0.999 && dot(delta / max(distanceFromSource, 0.0001), sector.xy) < sector.z) return 0;
    float angle = frac(atan2(delta.y, delta.x) / TWO_PI);
    float column = floor(angle * 1024 + 0.5);
    [unroll] for (int offset = -1; offset <= 1; offset++)
    {
        float2 sampleUV = float2((column + offset + 0.5) / 1024, (pulse + 0.5) / 32);
        float2 hit = SAMPLE_TEXTURE2D_LOD(_EchoSonarOcclusion, sampler_EchoSonarOcclusion, sampleUV, 0).rg;
        // Surface coverage is applied separately; the travelling line has no object exemption.
        if (distanceFromSource > hit.x + 0.03 && (objectId == 0 || abs(hit.y - objectId) > 0.25)) return 0;
    }
    return 1;
}

float EchoSurfaceVisible(float3 world, int pulse, float objectId)
{
    if (_EchoOcclusionEnabled == 0) return 1;
    float coverage = EchoVisible(world, pulse, objectId);
    if (coverage == 0) return 0;
    float2 delta = world.xz - _EchoSonarOrigins[pulse].xy;
    float distanceFromSource = length(delta);
    float column = floor(frac(atan2(delta.y, delta.x) / TWO_PI) * 1024 + 0.5);
    [unroll] for (int offset = -1; offset <= 1; offset++)
    {
        float2 sampleUV = float2((column + offset + 0.5) / 1024, (pulse + 0.5) / 32);
        float4 hit = SAMPLE_TEXTURE2D_LOD(_EchoSonarOcclusion, sampler_EchoSonarOcclusion, sampleUV, 0);
        if (objectId != 0 && abs(hit.y - objectId) <= 0.25)
            coverage = min(coverage, 1 - smoothstep(hit.z - max(0.001, hit.w), hit.z, distanceFromSource));
    }
    return coverage;
}
#endif

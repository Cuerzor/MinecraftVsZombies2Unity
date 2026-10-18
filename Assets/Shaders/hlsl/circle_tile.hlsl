half _CircleFill;
int _CircleTiled;
int _CircleStart;
int _CircleClockwise;
bool CheckCircleTile(float2 uv)
{
    float2 pos = uv - float2(0.5f, 0.5f);

    float clockwise = -1;
    if (_CircleClockwise)
    {
        clockwise = 1;
    }

    float2 angles[4];
    angles[0] = float2(pos.y * clockwise, -pos.x);
    angles[1] = float2(-pos.x * clockwise, pos.y);
    angles[2] = float2(-pos.y * clockwise, pos.x);
    angles[3] = float2(-pos.x * clockwise, -pos.y);

    float ang = degrees(atan2(angles[_CircleStart].x, angles[_CircleStart].y)) + 180;
    return _CircleFill * 360 > ang;
}
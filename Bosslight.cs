using Godot;
using System;


public partial class Bosslight : OmniLight3D
{

    public override void _Ready()
    {
        BossEvents.turnLightsOff += Off;
        BossEvents.turnLightsOn += On;
    }

    public override void _ExitTree()
    {
        BossEvents.turnLightsOff -= Off;
        BossEvents.turnLightsOn += On;
    }


    public void Off()
    {
        LightEnergy = 0;
    }

    public void On()
    {
        LightEnergy = 1;
    }

}

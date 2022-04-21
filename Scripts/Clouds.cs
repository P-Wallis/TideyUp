using Godot;
using System.Collections.Generic;
using TideyUp.Utils;

public class Clouds : Node2D
{
    public int cloudsToSpawn = 12;
    public float heightMin = -1200, heightMax = -50;
    public float left = -600, right = 600;
    public Texture[] cloudTextures = {
        GD.Load<Texture>("res://Sprites/Cloud_01.png"),
        GD.Load<Texture>("res://Sprites/Cloud_02.png"),
        GD.Load<Texture>("res://Sprites/Cloud_03.png")
        };

    private List<Cloud> clouds = new List<Cloud>();
    public override void _Ready()
    {
        for(int i=0; i< cloudsToSpawn; i++)
        {
            Cloud cloud = new Cloud();
            AddChild(cloud);
            cloud.Position = new Vector2(Random.Range(left, right), Random.Range(heightMin, heightMax));
            cloud.Init(this);
        }
    }
}
using Godot;
using System;

public enum SceneID
{
    Intro,
    Gameplay,
    Outro
}

public class SceneManager : Node2D
{
    public static SceneManager _;

    SceneManager()
    {
        if(_ == null)
        {
            _ = this;
        }
        else
        {
            Free();
        }
    }

    private Node loadedScene = null;
    private Node oldLoadedScene = null;
    public override void _Ready()
    {
        LoadScene(SceneID.Intro);
    }

    public void LoadScene(SceneID sceneID)
    {
        if(loadedScene!=null)
        {
            oldLoadedScene = loadedScene;
            CallDeferred(nameof(FreeOldScene));
        }

        PackedScene packedScene = null;
        switch(sceneID)
        {
            case SceneID.Intro:
	            packedScene = GD.Load<PackedScene>("res://Scenes/Intro.tscn");
                break;
            case SceneID.Gameplay:
	            packedScene = GD.Load<PackedScene>("res://Scenes/Gameplay.tscn");
            break;
            case SceneID.Outro:
	            packedScene = GD.Load<PackedScene>("res://Scenes/Outro.tscn");
            break;
        }
        if(packedScene !=null)
        {
            loadedScene = packedScene.Instance();
            if(loadedScene != null)
            {
                AddChild(loadedScene);
            }
        }
    }

    void FreeOldScene()
    {
        oldLoadedScene.Free();
    }

}

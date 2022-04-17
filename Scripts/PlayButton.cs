using Godot;
using System;

public class PlayButton : Button
{
    public override void _Ready()
    {
        Connect("pressed", this, nameof(OnPressed));
    }

    void OnPressed()
    {
        if(SceneManager._!=null)
        {
            SceneManager._.LoadScene(SceneID.Gameplay);
        }
    }
}

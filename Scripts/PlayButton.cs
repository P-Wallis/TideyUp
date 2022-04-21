using Godot;
using System;

public class PlayButton : Button
{
    private AudioStreamPlayer sound;
    public override void _Ready()
    {
        Connect("pressed", this, nameof(OnPressed));
        sound = SceneManager._.GetNode<AudioStreamPlayer>("ButtonSFX");
    }

    void OnPressed()
    {
        sound.Play();
        if(SceneManager._!=null)
        {
            SceneManager._.LoadScene(SceneID.Gameplay);
        }
    }
}

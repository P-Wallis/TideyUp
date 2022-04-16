using Godot;
using System;

public class PlankPreview : Node2D
{
    Sprite small, medium, large;
    public override void _Ready()
    {
        small = GetNode<Sprite>("Small");
        medium = GetNode<Sprite>("Medium");
        large = GetNode<Sprite>("Large");
    }

    public void SetSize(PlankSize size)
    {
        small.Hide();
        medium.Hide();
        large.Hide();

        switch(size)
        {
            case PlankSize.Small:
                small.Show();
                break;
            case PlankSize.Medium:
                medium.Show();
                break;
            case PlankSize.Large:
                large.Show();
                break;
        }
    }
}

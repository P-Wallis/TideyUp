using Godot;
using TideyUp.Utils;

public class Cloud : Sprite
{
    private Clouds clouds;
    private float speed;
    public void Init(Clouds clouds)
    {
        this.clouds = clouds;
        
        RandomizeParameters();
    }

    void RandomizeParameters()
    {
        speed = Random.Range(5f,20f);
        Texture = clouds.cloudTextures[Random.Range(0,clouds.cloudTextures.Length)];
    }

    public override void _Process(float delta)
    {
        Position -= new Vector2(speed * delta, 0);

        if(Position.x < clouds.left)
        {
            Position = new Vector2(clouds.right, Random.Range(clouds.heightMin, clouds.heightMax));
            RandomizeParameters();
        }
    }
}

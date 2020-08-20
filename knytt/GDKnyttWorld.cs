using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;
using YKnyttLib;

public class GDKnyttWorld : Node2D
{
    PackedScene area_scene;

    private Dictionary<string, string> directories { get; }
    public string MapPath { get; private set; }
    public string WorldConfigPath { get; private set; }
    //public Dictionary<KnyttPoint, GDKnyttArea> Areas { get; private set; }
    public KnyttRectPaging<GDKnyttArea> Areas { get; }

    public KnyttWorld<string> World { get; }

    public GDKnyttWorld()
    {
        this.Areas = new KnyttRectPaging<GDKnyttArea>(new KnyttPoint(1, 1));
        this.Areas.OnPageIn = (KnyttPoint loc) => instantiateArea(loc);
        this.Areas.OnPageOut = (KnyttPoint loc, GDKnyttArea area) => destroyArea(loc, area);

        this.World = new KnyttWorld<string>();
        this.area_scene = ResourceLoader.Load("res://knytt/GDKnyttArea.tscn") as PackedScene;
        this.directories = new Dictionary<string, string>();
    }

    public GDKnyttArea getArea(KnyttPoint area)
    {
        return this.Areas.Areas[area];
    }


    public GDKnyttArea instantiateArea(KnyttPoint point)
    {
        //if (this.Areas.ContainsKey(point)) { return this.Areas[point]; }

        var area = this.World.getArea(point);
        if (area == null) { return null; }

        var area_node = this.area_scene.Instance() as GDKnyttArea;
        area_node.loadArea(this, area);
        this.GetNode("Areas").AddChild(area_node);
        //this.Areas.Add(area.Position, area_node);

        return area_node;
    }

    public void destroyArea(KnyttPoint point, GDKnyttArea area)
    {
        if (area == null) { return; }
        area.QueueFree();
    }

    public void loadWorld(Directory world_dir)
    {
        this.discoverWorldStructure(world_dir);

        var map_file = new File();
        map_file.Open(this.MapPath, File.ModeFlags.Read);
        var data = map_file.GetBuffer((int)map_file.GetLen());
        map_file.Close();

        System.IO.MemoryStream map_stream = new System.IO.MemoryStream(data);
        
        this.World.parseWorldFiles(map_stream, null);
        GD.Print(this.World.getArea(new KnyttPoint(1000, 1000)));
    }

    public TileSet GetTileSet(int num)
    {
        // TODO: Check cache first

        // Check for override before loading internal
        string fname = this.World.TilesetsOverride[num];
        if (fname == null) { fname = string.Format("res://knytt/data/Tilesets/Tileset{0}.png", num); }
        var image = new Image();
        image.Load(fname);
        var texture = new ImageTexture();
        texture.CreateFromImage(image, (int)Texture.FlagsEnum.Repeat);

        return GDKnyttAssetBuilder.makeTileset(texture, false);
    }

    public Texture getGradient(int num)
    {
        // TODO: Check cache first

        string fname = this.World.TilesetsOverride[num];
        if (fname == null) { fname = string.Format("res://knytt/data/Gradients/Gradient{0}.png", num); }
        var image = new Image();
        image.Load(fname);
        var texture = new ImageTexture();
        texture.CreateFromImage(image, (int)Texture.FlagsEnum.Repeat);

        return texture;
    }

    public AudioStream getSong(int num)
    {
        if (num == 0) { return null; }
        string fname = this.World.MusicOverride[num];
        if (fname == null) { fname = string.Format("res://knytt/data/Music/Song{0}.ogg", num); }
        var f = new File();
        f.Open(fname, File.ModeFlags.Read);
        var buffer = f.GetBuffer((int)f.GetLen());
        f.Close();

        //var stream = new AudioStreamSample(); // This is for wav
        var stream = new AudioStreamOGGVorbis();
        stream.Data = buffer;
        stream.Loop = false;

        return stream;
    }

    public AudioStream getAmbiance(int num)
    {
        if (num == 0) { return null; }
        string fname = this.World.AmbianceOverride[num];
        if (fname == null) { fname = string.Format("res://knytt/data/Ambiance/Ambi{0}.ogg", num); }
        var f = new File();
        f.Open(fname, File.ModeFlags.Read);
        var buffer = f.GetBuffer((int)f.GetLen());
        f.Close();

        var stream = new AudioStreamOGGVorbis();
        stream.Data = buffer;
        stream.Loop = true;

        return stream;
    }

    private void discoverWorldStructure(Directory world_dir)
    {
        world_dir.ListDirBegin();
        while(true)
        {
            string name = world_dir.GetNext();
            if (name.Length == 0) { break; }
            string lname = name.ToLower();
            if (world_dir.CurrentIsDir())
            {
                var dname = world_dir.GetCurrentDir() + "/" + name;
                this.directories.Add(lname, dname);
                if (lname.Equals("tilesets")) { checkAssetOverrides(dname, this.World.TilesetsOverride); }
                else if (lname.Equals("ambiance")) { checkAssetOverrides(dname, this.World.AmbianceOverride); }
                else if (lname.Equals("gradients")) { checkAssetOverrides(dname, this.World.GradientsOverride); }
                else if (lname.Equals("music")) { checkAssetOverrides(dname, this.World.MusicOverride); }
            }
            else if (name.ToLower().Equals("map.bin")) { this.MapPath = world_dir.GetCurrentDir() + "/" + name; }
            else if (name.ToLower().Equals("world.ini")) { this.WorldConfigPath = world_dir.GetCurrentDir() + "/" + name; }
        }
        world_dir.ListDirEnd();
    }

    private void checkAssetOverrides(string dname, string[] overrides)
    {
        var asset_dir = new Directory();
        asset_dir.Open(dname);
        asset_dir.ListDirBegin();

        while(true)
        {
            string name = asset_dir.GetNext();
            if (name.Length == 0) { break; }
            if (asset_dir.CurrentIsDir()) { continue; }
            var num = Regex.Match(name, @"\d+").Value;
            //GD.Print("'" + name + "' " + num);
            this.World.TilesetsOverride[int.Parse(num)] = asset_dir.GetCurrentDir() + "/" + name;
        }

        asset_dir.ListDirEnd();
    }
}

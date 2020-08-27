using Godot;
using YKnyttLib;

public class LevelSelection : CanvasLayer
{
    KnyttWorldManager<Texture> Manager { get; }
    PackedScene info_scene;

    string filter_category;
    string filter_difficulty;
    string filter_size;

    public LevelSelection()
    {
        Manager = new KnyttWorldManager<Texture>();
    }

    public override void _Ready()
    {
        this.info_scene = ResourceLoader.Load<PackedScene>("res://knytt/ui/InfoScreen.tscn");
        this.discoverWorlds("./worlds");
        this.listWorlds();
    }

    // Search the given directory for worlds
    // TODO: This should do a very different process for bin files
    private void discoverWorlds(string path)
    {
        var wd = new Directory();
        wd.Open(path);

        wd.ListDirBegin();
        while(true)
        {
            string name = wd.GetNext();
            if (name.Length == 0) { break; }
            
            if (wd.CurrentIsDir())
            {
                if (!verifyDirWorld(wd, name)) { continue; }
                var icon = GDKnyttAssetManager.loadExternalTexture(wd.GetCurrentDir() + "/" + name + "/Icon.png");
                var txt = GDKnyttAssetManager.loadTextFile(wd.GetCurrentDir() + "/" + name + "/World.ini");
                GDKnyttWorldImpl world = new GDKnyttWorldImpl();
                world.setDirectory(wd.GetCurrentDir() + "/" + name, name);
                world.loadWorldConfig(txt);
                Manager.addWorld(world, icon);
            }
            else
            {
                if (!name.EndsWith(".knytt.bin")) { continue; }
                var binloader = new KnyttBinWorldLoader(GDKnyttAssetManager.loadFile(wd.GetCurrentDir() + "/" + name));
                var icon = GDKnyttAssetManager.loadTexture(binloader.GetFile("Icon.png"));
                var txt = GDKnyttAssetManager.loadTextFile(binloader.GetFile("World.ini"));
                GDKnyttWorldImpl world = new GDKnyttWorldImpl();
                world.setDirectory(wd.GetCurrentDir() + "/" + name, binloader.RootDirectory);
                world.loadWorldConfig(txt);
                world.setBinMode(null);
                Manager.addWorld(world, icon);
            }
        }
        wd.ListDirEnd();
    }

    private bool verifyDirWorld(Directory dir, string name)
    {
        if (name.Equals(".") || name.Equals("..")) { return false; }

        // TODO: Check for file signatures
        return true;
    }

    private void listWorlds()
    {
        // Should take filter settings into account
        var worlds = Manager.filter(filter_category, filter_difficulty, filter_size);
        var game_container = GetNode<GameContainer>("MainContainer/ScrollContainer/GameContainer");
        game_container.clearWorlds();

        foreach(var world_entry in worlds)
        {
            game_container.addWorld((GDKnyttWorldImpl)world_entry.world, world_entry.extra_data);
        }
    }

    public void _on_BackButton_pressed()
    {
        GetNodeOrNull<AudioStreamPlayer>("../MenuClickPlayer")?.Play();
        this.QueueFree();
    }

    public void _on_GamePressed(GameButton button)
    {
        GetNodeOrNull<AudioStreamPlayer>("../MenuClickPlayer")?.Play();
        var info = info_scene.Instance() as InfoScreen;
        info.initialize(button.KWorld);
        this.GetParent().AddChild(info);
    }

    public void _on_CategoryDropdown_item_selected(int index)
    {
        var dropdown = GetNode<OptionButton>("MainContainer/FilterContainer/CategoryDropdown");
        filter_category = dropdown.Text.Equals("[All]") ? null : dropdown.Text;
        this.listWorlds();
    }

    public void _on_DifficultyDropdown_item_selected(int index)
    {
        var dropdown = GetNode<OptionButton>("MainContainer/FilterContainer/DifficultyDropdown");
        filter_difficulty = dropdown.Text.Equals("[All]") ? null : dropdown.Text;
        this.listWorlds();
    }

    public void _on_SizeDropdown_item_selected(int index)
    {
        var dropdown = GetNode<OptionButton>("MainContainer/FilterContainer/SizeDropdown");
        filter_size = dropdown.Text.Equals("[All]") ? null : dropdown.Text;
        this.listWorlds();
    }
}

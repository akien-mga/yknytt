using Godot;
using YKnyttLib;

public class InfoScreen : CanvasLayer
{
    public GDKnyttWorldImpl KWorld { get; private set; }

    public void initialize(GDKnyttWorldImpl world)
    {
        this.KWorld = world;
        if (world.BinMode) { world.setBinMode(new KnyttBinWorldLoader(GDKnyttAssetManager.loadFile(world.WorldDirectory))); }
        var info = world.getWorldTexture("Info.png");
        GetNode<TextureRect>("InfoRect").Texture = (Texture)info;
        GetNode<SlotButton>("Slot1Button").BaseFile = "user://Saves/" + world.WorldDirectoryName;
        GetNode<SlotButton>("Slot2Button").BaseFile = "user://Saves/" + world.WorldDirectoryName;
        GetNode<SlotButton>("Slot3Button").BaseFile = "user://Saves/" + world.WorldDirectoryName;
    }

    public void _on_BackButton_pressed()
    {
        ClickPlayer.Play();
        KWorld.purgeBinFile();
        this.QueueFree();
    }

    public void closeOtherSlots(int slot)
    {
        closeSlotIfNot(GetNode<SlotButton>("Slot1Button"), slot);
        closeSlotIfNot(GetNode<SlotButton>("Slot2Button"), slot);
        closeSlotIfNot(GetNode<SlotButton>("Slot3Button"), slot);
    }

    private void closeSlotIfNot(SlotButton sb, int slot)
    {
        if (sb.slot != slot) { sb.close(); }
    }

    public void _on_SlotButton_StartGame(bool new_save, string filename, int slot)
    {
        GetNode<LevelSelection>("../LevelSelection").killConsumers();
        string fname = new_save ? KWorld.WorldDirectory + "/DefaultSavegame.ini" : filename;
        
        KnyttSave save = new KnyttSave(KWorld, 
                         new_save ? GDKnyttAssetManager.loadTextFile(KWorld.getWorldData("DefaultSavegame.ini")) :
                                    GDKnyttAssetManager.loadTextFile(filename), 
                                    slot);
        
        KWorld.CurrentSave = save;
        GDKnyttDataStore.KWorld = KWorld;
        GDKnyttDataStore.startGame(new_save);
        this.QueueFree();
    }

}

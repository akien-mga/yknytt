using Godot;

public class SlotButton : Button
{
    [Export]
    public int slot = 1;

    [Signal]
    public delegate void StartGame();

    public bool ConfirmActive
    {
        get { return GetNode<TextureRect>("ConfirmArrow").Visible; }
        set { GetNode<TextureRect>("ConfirmArrow").Visible = value; }
    }

    public bool EraseActive
    {
        get { return GetNode<TextureRect>("EraseArrow").Visible; }
        set { GetNode<TextureRect>("EraseArrow").Visible = value; }
    }

    public bool StartLoadActive
    {
        get { return GetNode<TextureRect>("StartLoadArrow").Visible; }
        set { GetNode<TextureRect>("StartLoadArrow").Visible = value; }
    }

    // Dir/filename without slot # or .ini
    string _base_file;
    public string BaseFile
    {
        get { return _base_file; }
        set 
        { 
            _base_file = value;  
            checkSlot();
        }
    }

    public bool NewMode { get; private set; }

    public File SlotFile
    {
        get 
        {
            var f = new File();
            f.Open(FullFilename, File.ModeFlags.Read);
            return f;
        }
    }

    public string FullFilename { get { return string.Format("{0} {1}.ini", BaseFile, slot); } }

    public override void _Ready()
    {
        Text = string.Format("Slot {0}", this.slot);
        close();
    }

    // Check the slot and configure
    public void checkSlot()
    {
        close();

        var f = SlotFile;
        if (f.IsOpen()) { setupLoadMode(); }
        else { setupNewMode(); }
        f.Close();
        
    }

    private void setupNewMode()
    {
        NewMode = true;
        GetNode<Button>("StartLoadArrow/StartLoadButton").Text = "Start New Game";
    }

    private void setupLoadMode()
    {
        NewMode = false;
        GetNode<Button>("StartLoadArrow/StartLoadButton").Text = "Load Game";
    }

    public void close()
    {
        ConfirmActive = false;
        EraseActive = false;
        StartLoadActive = false;
    }

    public void _on_SlotButton_pressed()
    {
        GetNodeOrNull<AudioStreamPlayer>("../../MenuClickPlayer")?.Play();
        GetParentOrNull<InfoScreen>()?.closeOtherSlots(this.slot);
        StartLoadActive = true;
        if (!NewMode) { EraseActive = true; }
    }

    public void _on_EraseButton_pressed()
    {
        GetNodeOrNull<AudioStreamPlayer>("../../MenuClickPlayer")?.Play();
        ConfirmActive = true;
    }

    public void _on_ConfirmButton_pressed()
    {
        GetNodeOrNull<AudioStreamPlayer>("../../MenuClickPlayer")?.Play();
        var dir = new Directory();
        dir.Remove(FullFilename);
        checkSlot();
    }

    public void _on_StartLoadButton_pressed()
    {
        GetNodeOrNull<AudioStreamPlayer>("../../MenuClickPlayer")?.Play();
        // Message up to the level selection with a new? flag, the filename, and the slot number
        EmitSignal(nameof(StartGame), NewMode, FullFilename, slot);
    }
}
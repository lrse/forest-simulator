public class ElevationMap : ITaskIO {

    public int Width { get; private set; }
    public int Height { get; private set; }
    public long Size { get; private set; }
    public float MaxElevation { get; private set; }
    public float MinElevation { get; private set; }
    public readonly float NO_VALUE = -9999;

    private float[,] cells;

    public ElevationMap(int width, int height) {
        this.Width = width;
        this.Height = height;
        this.Size = width * height;
        this.cells = new float[height, width];
        this.MaxElevation = NO_VALUE;
        this.MinElevation = NO_VALUE;
        PopulateWithNoValue();
    }

    public float this[int row, int col] {
        get {
            return cells[row, col];
        }
        set {
            cells[row, col] = value;
            if (MaxElevation == NO_VALUE || value > MaxElevation) {
                MaxElevation = value;
            }
            if (MinElevation == NO_VALUE || value < MinElevation) {
                MinElevation = value;
            }
        }
    }

    private void PopulateWithNoValue() {
        for (int i = 0; i < Height; i++) {
            for (int j = 0; j < Width; j++) {
                cells[i, j] = NO_VALUE;
            }
        }
    }

}

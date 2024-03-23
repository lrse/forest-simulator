using System.Collections.Generic;
using System.Linq;

public class DetectionResult : ITaskIO {

    public int Width { get; private set; }
    public int Height { get; private set; }

    private Dictionary<string, Tree> trees = new Dictionary<string, Tree>();

    public DetectionResult(int width, int height) {
        this.Width = width;
        this.Height = height;
    }

    public void AssignPixelToTree(string treeName, int row, int col, float altitude) {
        if (!trees.ContainsKey(treeName)) {
            trees[treeName] = new Tree();
        }

        trees[treeName].AssignPixel(row, col, altitude);
    }

    public List<Tree> GetTrees() {
        return trees.Values.ToList();
    }

    public class Tree {

        private float highestAltitude = 0;
        public int HighestAltitudeRow { get; private set; }
        public int HighestAltitudeCol { get; private set; }

        private HashSet<(int, int)> assignedPixels = new HashSet<(int, int)>();

        public void AssignPixel(int row, int col, float altitude) {
            if (altitude > highestAltitude) {
                highestAltitude = altitude;
                HighestAltitudeRow = row;
                HighestAltitudeCol = col;
            }

            assignedPixels.Add((row, col));
        }

        public bool IsPixelBorder(int row, int col) {
            return DoesPixelBelongToTree(row, col) && IsAnyNeighboorNotSet(row, col);
        }

        public HashSet<(int, int)> GetAssignedPixels() {
            return assignedPixels;
        }

        private bool IsAnyNeighboorNotSet(int row, int col) {
            return !DoesPixelBelongToTree(row - 1, col - 1) ||
                !DoesPixelBelongToTree(row, col - 1) ||
                !DoesPixelBelongToTree(row + 1, col - 1) ||
                !DoesPixelBelongToTree(row + 1, col) ||
                !DoesPixelBelongToTree(row + 1, col + 1) ||
                !DoesPixelBelongToTree(row, col + 1) ||
                !DoesPixelBelongToTree(row - 1, col + 1) ||
                !DoesPixelBelongToTree(row - 1, col);
        }

        private bool DoesPixelBelongToTree(int row, int col) {
            return assignedPixels.Contains((row, col));
        }

    }
}

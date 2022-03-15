namespace CFisobs.Creatures
{
    public struct TileResist
    {
        public PathCost OffScreen; // when abstracted
        public PathCost Floor;
        public PathCost Corridor;
        public PathCost Climb;
        public PathCost Wall;
        public PathCost Ceiling;
        public PathCost Air;
        public PathCost Solid;
    }
}

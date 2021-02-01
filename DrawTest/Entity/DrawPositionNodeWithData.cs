using System.Collections.Generic;

namespace DrawTest.Entity
{
    public class DrawPositionNodeWithData
    {
        public Node<DrawPositionNode> Node { get; set; }

        public List<Player> Players { get; set; }
    }
}

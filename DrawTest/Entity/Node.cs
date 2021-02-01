namespace DrawTest.Entity
{
    public class Node<T>
    {
        private T data;
        private Node<T> leftChild;//左子节点
        private Node<T> rightChild;//右子节点
        private Node<T> parent;

        public Node(T data, Node<T> ln, Node<T> rn)
        {
            this.data = data;
            leftChild = ln;
            rightChild = rn;
        }

        public Node(Node<T> ln, Node<T> rn)
        {
            data = default;
            leftChild = ln;
            rightChild = rn;
        }

        public Node(T data)
        {
            this.data = data;
            leftChild = null;
            rightChild = null;
        }

        public Node()
        {
            data = default;
            leftChild = null;
            rightChild = null;
        }

        public T Data
        {
            get => data;
            set => data = value;
        }

        public Node<T> LeftChild
        {
            get => leftChild;
            set => leftChild = value;
        }

        public Node<T> RightChild
        {
            get => rightChild;
            set => rightChild = value;
        }

        public Node<T> Parent
        {
            get => parent;
            set => parent = value;
        }
    }
}

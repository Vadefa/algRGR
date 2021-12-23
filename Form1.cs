using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace algLab4
{
    public partial class Form1 : Form
    {
        StorService storage;
        Graphics paintForm;
        int a;
        public Form1()
        {
            InitializeComponent();
            
            paintForm = CreateGraphics();
            storage = new StorService();
            a = 1;
        }

        public class Edge
        {
            private Point p1;
            private Point p2;
            
            private Pen defaultPen = new Pen(Color.Black, 2);
            private Pen selectedPen = new Pen(Color.Red, 6);        // для выделения

            public bool is_gone = false;
            public bool is_printed = false;
            private List<Ver> vers;

            public bool is_Connected(Ver ver)
            {
                return vers.Contains(ver);
            }
            public bool are_Connected(Ver ver1, Ver ver2)
            {
                bool a = vers.Contains(ver1);
                bool b = vers.Contains(ver2);
                return (a && b);
            }
            
            public void paint(Graphics paintForm)
            {
                    paintForm.DrawLine(defaultPen, p1, p2);
            }
            public Edge(Point p1, Point p2, Ver ver1, Ver ver2, Graphics paintForm)
            {
                this.p1 = p1;
                this.p2 = p2;

                p1.X += ver1.r;
                p1.Y += ver1.r;
                p2.X += ver2.r;
                p2.Y += ver2.r;

                vers = new List<Ver>() { ver1, ver2 };
                paint(paintForm);
            }
        }

        public class Ver
        {
            private Rectangle rect;
            private int x;
            private int y;
            public int r = 30;
            private bool is_focused;

            public string name;
            private int name_x;
            private int name_y;
            private int name_size;

            Pen defaultPen = new Pen(Color.Black, 4);
            Pen focusedPen = new Pen(Color.Blue, 4);
            SolidBrush defaultBrush = new SolidBrush(Color.White);
            SolidBrush focusedBrush = new SolidBrush(Color.LightSkyBlue);

            public List<Edge> edges = new List<Edge>();
            public List<Ver> neighbours = new List<Ver>();

            public bool is_visited = false;

            public bool is_articulation = false;
            public bool is_visible = true;

            //drawing method
            public void paint(Graphics paintForm)
            {
                if (is_focused == true || is_articulation == true)
                {
                    paintForm.DrawEllipse(focusedPen, rect);
                    paintForm.FillEllipse(focusedBrush, rect);
                }
                else
                {
                    paintForm.DrawEllipse(defaultPen, rect);
                    paintForm.FillEllipse(defaultBrush, rect);
                }

                paintForm.DrawString(name, new Font("Arial", name_size), new SolidBrush(Color.Black), name_x, name_y);
            }
            public Point getPos()
            {
                return new Point(x + r + ((int)(focusedPen.Width / 2)), y + r + ((int)(focusedPen.Width / 2)));
            }

            public void edgeRemove(Edge edge)
            {
                edges.Remove(edge);
            }
            public void edgeGone(Ver v)
            {
                foreach (Edge e in edges)
                {
                    if (e.are_Connected(this, v))
                    {
                        e.is_gone = true;
                        break;
                    }
                }
            }
            public void edgePrinted(Ver v)
            {
                foreach (Edge e in edges)
                {
                    if (e.are_Connected(this, v))
                    {
                        e.is_printed = true;
                        break;
                    }
                }
            }
            public bool checkEdgeGone(Ver v)
            {
                foreach(Edge e in edges)
                {
                    if (e.are_Connected(this, v))
                        if (e.is_gone == true)
                            return true;
                }
                return false;
            }
            public void neighbourRemove(Ver ver, List<Edge> edges)
            {
                foreach (Edge edge in this.edges)
                {
                    if (edge.is_Connected(ver))
                    {
                        edgeRemove(edge);
                        edges.Remove(edge);
                        break;
                    }
                }
                neighbours.Remove(ver);
            }

            // focus
            public bool focusCheck()
            {
                if (is_focused == true)
                    return true;
                else
                    return false;
            }
            public void focus(Graphics paintForm)
            {
                is_focused = true;
                paint(paintForm);
            }
            public void unfocus()
            {
                is_focused = false;
                ActiveForm.Invalidate();
            }


            // something with coordinates
            public bool checkUnderMouse(Graphics paintForm, int x_mouse, int y_mouse)
            {
                int x0 = x;
                int y0 = y;

                int x1 = x + r * 2 + ((int)(defaultPen.Width / 2));
                int y1 = y + r * 2 + ((int)(defaultPen.Width / 2));

                if ((x_mouse >= x0) && (x_mouse <= x1) && (y_mouse >= y0) && (y_mouse <= y1))
                    return true;
                else
                    return false;
            }

            public void addEdge(Edge edge)
            {
                edges.Add(edge);
            }
            public void addNeighbour(Ver ver)
            {
                neighbours.Add(ver);
            }

            public bool neighbourCheck(Ver ver)
            {
                return neighbours.Contains(ver);
            }

            public Ver(int x, int y, Graphics paintForm, string a)
            {
                this.x = x - r - ((int)(focusedPen.Width / 2));
                this.y = y - r - ((int)(focusedPen.Width / 2));
                is_focused = true;
                rect = new Rectangle(this.x, this.y, r * 2, r * 2);

                name = a;
                name_size = 14;
                name_x = x - name_size + ((int)(focusedPen.Width / 2));
                name_y = y - name_size;

                paint(paintForm);
                
            }
        }

        public class MyStorage
        {
            protected Ver[] storage;
            protected List<Edge> edges = new List<Edge>();
            protected int iter;
            protected int size;
            protected int count;

            public void remove()                            // removes all nulled elements
            {
                int del = 0;

                for (int i = 0; i < size; i++)
                    if (storage[i] != null)
                        del = del + 1;

                Ver[] tempStorage = new Ver[del];           // here we'll put elements that should remain

                int j = 0;
                for (int i = 0; i < size; i++)
                    if (storage[i] != null)
                    {
                        tempStorage[j] = storage[i];        // putting remaining elements
                        j = j + 1;
                    }

                size = del;                                 // changing properties
                count = size;
                iter = size;
                if (iter < 0)
                    iter = 0;

                storage = new Ver[size];
                for (int i = 0; i < size; i++)
                    storage[i] = tempStorage[i];            // moved all remained elements
            }
            private void sizeImprove()
            {
                Ver[] tempStorage = storage;
                size = size + 1;
                storage = new Ver[size];

                for (int i = 0; i < size - 1; i++)
                    storage[i] = tempStorage[i];
                storage[size - 1] = null;

            }

            public void add(Ver circle, Graphics ellipses)
            {
                if (count != 0)
                    foreach (Ver c in storage)
                        c.unfocus();

                if (iter < size)
                {
                    if (storage[iter] == null)
                    {
                        storage[iter] = circle;
                        iter = iter + 1;
                    }
                }
                else if (iter == size)
                {
                    sizeImprove();
                    storage[iter] = circle;
                    iter = iter + 1;
                }
                count = count + 1;
            }
            public MyStorage()
            {
                iter = 0;
                count = 0;
                size = 1;
                storage = new Ver[size];
            }
        }

        public class StorService : MyStorage
        {
            public void removeFocused(Graphics paintForm)
            {
                for (int i = 0; i < size; i++)
                {
                    if (storage[i].focusCheck() == true)
                    {
                        foreach (Ver ver in storage)
                            ver.neighbourRemove(storage[i], edges);


                        storage[i] = null;                  // placing null in the storage at the elements we should delete
                    }
                }

                remove();
                //now at the form's paint event we won't draw elements those were focused. Let's make the form repaint it immediately.
                ActiveForm.Invalidate();
            }

            public void focusOnClick(Graphics paintForm, int x_mouse, int y_mouse)
            {
                if (count == 0)
                    return;

                int i = size;
                bool found = false;
                while ((found == false) && (i > 0))
                {
                    i = i - 1;
                    found = storage[i].checkUnderMouse(paintForm, x_mouse, y_mouse);
                }

                if (found == true)
                {
                    storage[i].focus(paintForm);             // выделили вершину, на которую нажали

                    int j = 0;
                    found = false;                          // проверка, есть ли ещё выделенные вершины
                    while (found == false && j < size)
                    {
                        if (i != j && storage[j].focusCheck() == true)
                        {
                            found = true;
                            continue;
                        }
                        j++;
                    }

                    if (j < size)
                    {                                       // нужно создать ребро

                        storage[i].unfocus();
                        storage[j].unfocus();

                        foreach (Edge e in edges)
                        {
                            if (e.are_Connected(storage[i], storage[j]))
                                return;
                        }

                        Edge edge = new Edge(storage[i].getPos(), storage[j].getPos(), storage[i], storage[j], paintForm);
                        storage[i].addEdge(edge);
                        storage[j].addEdge(edge);
                        edges.Add(edge);

                        storage[i].addNeighbour(storage[j]);
                        storage[j].addNeighbour(storage[i]);

                    }
                }
                else
                    foreach (Ver c in storage)
                        c.unfocus();
            }
            public void paint(Graphics paintForm)
            {
                foreach (Edge e in edges)
                    e.paint(paintForm);

                if (count != 0)
                    foreach (Ver circle in storage)
                        circle.paint(paintForm);
            }

            public void inDepth(Stack<Ver> stec, List<Ver> vis)
            {
                Ver ver;
                while (stec.Count != 0)
                {
                    ver = stec.Pop();
                    ver.is_visited = true;
                    if (vis.Contains(ver) == false)
                        vis.Add(ver);
                    bool has_neighbours = false;
                    foreach (Ver v in ver.neighbours)
                    {
                        if (v.is_visited == false && v.is_visible == true)
                        {
                            has_neighbours = true;
                            break;
                        }
                    }
                    if (has_neighbours == true)
                        foreach (Ver v in ver.neighbours)
                            if (v.is_visited == false && v.is_visible == true)
                                stec.Push(v);
                }
                foreach (Ver v in storage)
                    v.is_visited = false;
            }

            public string inDepthSearch(Graphics paintForm)
            {
                foreach (Ver v in storage)                  // занулим точки сочленения перед поиском новых точек
                    v.is_articulation = false;


                Stack<Ver> stec = new Stack<Ver>();
                List<Ver> vis = new List<Ver>();
                stec.Push(storage[0]);

                Ver ver;
                while (stec.Count != 0)
                {
                    ver = stec.Pop();
                    ver.is_visited = true;
                    if (vis.Contains(ver) == false)
                        vis.Add(ver);
                    bool has_neighbours = false;
                    foreach (Ver v in ver.neighbours)
                    {
                        if (v.is_visited == false)
                        {
                            has_neighbours = true;
                            break;
                        }
                    }
                    if (has_neighbours == true)
                        foreach (Ver v in ver.neighbours)
                            if (v.is_visited == false)
                                stec.Push(v);
                }

                if (this.count != vis.Count)
                {
                    MessageBox.Show("Граф несвязный.");
                    return "";
                }

                foreach (Ver v in storage)
                    v.is_visited = false;

                int count = 0;
                for (int i = 0; i < vis.Count; i++)
                {
                    storage[i].is_visible = false;
                    for (int j = 0; j < vis.Count; j++)
                    {
                        if (i == j)
                            continue;
                        else
                        {
                            Stack<Ver> st = new Stack<Ver>();
                            List<Ver> l = new List<Ver>();
                            st.Push(storage[j]);
                            inDepth(st, l);
                            if (l.Count != (vis.Count - 1))
                            {
                                storage[i].is_articulation = true;
                                count++;
                                break;
                            }    
                        }
                    }
                    storage[i].is_visible = true;
                }
                paint(paintForm);


                return "Всего точек сочленения " + count.ToString();
            }
        }
        ///////// ended up for the storages and Ver classes



        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            paintForm = CreateGraphics();
            storage.paint(paintForm);
        }

        private void Form1_DoubleClick(object sender, EventArgs e)
        {
            //PointToClient returns mouse position in relation to the form, not to the screen
            Point mousePos = PointToClient(new Point(Cursor.Position.X, Cursor.Position.Y));
            storage.add(new Ver(mousePos.X, mousePos.Y, paintForm, a.ToString()), paintForm);
            a++;
        }


        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point mousePos = PointToClient(new Point(Cursor.Position.X, Cursor.Position.Y));
                storage.focusOnClick(paintForm, mousePos.X, mousePos.Y);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            storage.removeFocused(paintForm);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label1.Text = storage.inDepthSearch(paintForm);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            storage.remove();
            storage = new StorService();
            a = 1;
            ActiveForm.Invalidate();
            label1.Text = "";
        }
    }
}

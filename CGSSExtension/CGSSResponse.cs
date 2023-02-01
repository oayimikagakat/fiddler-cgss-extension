using System.Text;
using System.Windows.Forms;
using Fiddler;

namespace CGSSExtension
{
    public class CGSSResponse : Inspector2, IResponseInspector2
    {
        TreeView treeView;
        private byte[] _body;

        public CGSSResponse() { }

        public HTTPResponseHeaders headers { get; set; }

        public byte[] body
        {
            get
            {
                return _body;
            }
            set
            {
                _body = value;

                if (body != null)
                {
                    string cgssbody = Encoding.UTF8.GetString(body);
                    CGSSUtil.SetJsonTreeView(treeView, CGSSUtil.DecryptBody(cgssbody, CGSSRequest.udid));
                }
            }
        }

        public bool bDirty { get { return false; } }

        public bool bReadOnly { get; set; }

        public void Clear()
        {
            body = null;
            treeView.Text = "";
        }

        public override void AddToTab(TabPage o)
        {
            treeView = new TreeView
            {
                Height = o.Height
            };
            o.Text = "CGSS Response";
            o.Controls.Add(treeView);
            o.Controls[0].Dock = DockStyle.Fill;
        }

        public override int GetOrder()
        {
            return 150;
        }
    }
}

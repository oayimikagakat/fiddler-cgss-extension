using System.Text;
using System.Windows.Forms;
using Fiddler;

[assembly: RequiredVersion("5.0.0.0")]

namespace CGSSExtension
{
    public class CGSSRequest : Inspector2, IRequestInspector2
    {
        TreeView treeView;
        private HTTPRequestHeaders _headers;
        private byte[] _body;
        public static string udid = "";

        public CGSSRequest() { }

        public HTTPRequestHeaders headers
        {
            get
            {
                return _headers;
            }
            set
            {
                _headers = value;

                if (headers.Exists("UDID"))
                {
                    udid = CGSSUtil.Deobfuscate(headers["UDID"]);
                }
            }
        }

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
                    CGSSUtil.SetJsonTreeView(treeView, CGSSUtil.DecryptBody(cgssbody, udid));
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
            o.Text = "CGSS Request";
            o.Controls.Add(treeView);
            o.Controls[0].Dock = DockStyle.Fill;
        }

        public override int GetOrder()
        {
            return 150;
        }
    }
}

using System.Text;
using System.Windows.Forms;
using Fiddler;


namespace CGSSExtension
{
    public class CGSSRequest : Inspector2, IRequestInspector2
    {
        TextBox textbox;
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
                    textbox.Text = CGSSUtil.DecryptBody(cgssbody, udid);
                }
            }
        }

        public bool bDirty { get { return false; } }

        public bool bReadOnly { get; set; }

        public void Clear()
        {
            body = null;
            textbox.Text = "";
        }

        public override void AddToTab(TabPage o)
        {
            textbox = new TextBox
            {
                Height = o.Height,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            o.Text = "CGSS Request";
            o.Controls.Add(textbox);
            o.Controls[0].Dock = DockStyle.Fill;
        }

        public override int GetOrder()
        {
            return 150;
        }
    }
}

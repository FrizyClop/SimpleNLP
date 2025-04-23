namespace SimpleNLP
{
    public class Text
    {
        private string _title;
        private string _class;
        private string _content;

        public string Title { get { return _title; } set { _title = value; } }
        public string Content { get { return _content; } set { _content = value; } }
        public string ClassOfText { get { return _class; } set { _class = value; } }

        public Text(string Title, string Content)
        {
            _title = Title;
            _content = Content;
        }

        public Text(string Title, string ClassOfText, string Content)
        {
            _title = Title;
            _class = ClassOfText;
            _content = Content;
        }

        public override string ToString()
        {
            if(ClassOfText == null)
            {
                return _title + "\nКласс: -";
            }
            else
            {
                return _title + "\nКласс: " + ClassOfText;
            }
        }
    }
}

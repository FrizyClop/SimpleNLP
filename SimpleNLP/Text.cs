namespace SimpleNLP
{
    public class Text
    {
        private string _title;
        private string _class;
        private string _content;
        private bool _isPreprocessed;

        public string Title { get { return _title; } set { _title = value; } }
        public string Content { get { return _content; } set { _content = value; } }
        public string ClassOfText { get { return _class; } set { _class = value; } }
        public bool IsPreprocessed { get { return _isPreprocessed; } set { _isPreprocessed = value; } }

        public Text(string Title, string Content, bool NeedPreprocessing)
        {
            _title = Title;
            _content = Content;
            _isPreprocessed = !NeedPreprocessing;
        }

        public Text(string Title, string ClassOfText, string Content, bool NeedPreprocessing)
        {
            _title = Title;
            _class = ClassOfText;
            _content = Content;
            _isPreprocessed = !NeedPreprocessing;
        }

        public override string ToString()
        {
            return _title + ClassToString() + IsPreprocessedToString();
        }

        private string ClassToString()
        {
            if (ClassOfText == null)
                return "\nКласс: -";
            else
                return "\nКласс: " + ClassOfText;
        }

        private string IsPreprocessedToString()
        {
            if (_isPreprocessed)
                return "\n Требуется предобработка: Нет";
            else
                return "\n Требуется предобработка: Да";
        }
    }
}

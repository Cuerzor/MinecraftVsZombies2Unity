#nullable enable

using System;
using System.Globalization;

namespace ExpressionEvaluator
{
    class Tokenizer
    {
        private readonly string _text;
        private int _pos;

        public Tokenizer(string text)
        {
            _text = text;
        }

        public Token Next()
        {
            SkipWhite();

            if (_pos >= _text.Length)
                return new Token { Type = TokenType.End };

            char c = _text[_pos];

            if (char.IsDigit(c) || c == '.')
                return ReadNumber();

            if (c == '"')
                return ReadString();

            if (char.IsLetter(c) || c == '_')
                return ReadIdentifier();

            _pos++;
            switch (c)
            {
                case '+': return new Token { Type = TokenType.Plus };
                case '-': return new Token { Type = TokenType.Minus };
                case '*': return new Token { Type = TokenType.Mul };
                case '/': return new Token { Type = TokenType.Div };
                case '(': return new Token { Type = TokenType.LParen };
                case ')': return new Token { Type = TokenType.RParen };
                case ',': return new Token { Type = TokenType.Comma };
            }

            throw new Exception($"Unexpected char: {c}");
        }

        private void SkipWhite()
        {
            while (_pos < _text.Length && char.IsWhiteSpace(_text[_pos]))
                _pos++;
        }

        private Token ReadNumber()
        {
            int start = _pos;
            while (_pos < _text.Length && (char.IsDigit(_text[_pos]) || _text[_pos] == '.'))
                _pos++;

            string s = _text.Substring(start, _pos - start);
            return new Token
            {
                Type = TokenType.Number,
                Number = double.Parse(s, CultureInfo.InvariantCulture)
            };
        }

        private Token ReadString()
        {
            _pos++; // skip "
            int start = _pos;
            var sb = new System.Text.StringBuilder();

            while (_pos < _text.Length)
            {
                char c = _text[_pos];

                // 处理转义：\" -> "
                if (c == '\\' && _pos + 1 < _text.Length && _text[_pos + 1] == '"')
                {
                    sb.Append('"');
                    _pos += 2; // 跳过 \" 两个字符
                    continue;
                }

                // 未转义的 " 才是字符串结束
                if (c == '"')
                    break;

                sb.Append(c);
                _pos++;
            }

            _pos++; // skip closing "

            return new Token { Type = TokenType.String, Text = sb.ToString() };
        }

        private Token ReadIdentifier()
        {
            int start = _pos;
            while (_pos < _text.Length && (char.IsLetterOrDigit(_text[_pos]) || _text[_pos] == '_'))
                _pos++;

            return new Token
            {
                Type = TokenType.Identifier,
                Text = _text.Substring(start, _pos - start)
            };
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace ArkeIndustries.VisualRust
{
    [Export(typeof(ISmartIndentProvider))]
    [ContentType("rust")]
    sealed class VisualRustSmartIndentProvider : ISmartIndentProvider
    {
        public ISmartIndent CreateSmartIndent(ITextView textView)
        {
            return new VisualRustSmartIndent(textView);
        }
    }

    internal sealed class VisualRustSmartIndent : ISmartIndent
    {
        private ITextView _textView;
        internal VisualRustSmartIndent(ITextView textView)
        {
            _textView = textView;
        }

        int? ISmartIndent.GetDesiredIndentation(ITextSnapshotLine line)
        {
            var snap = _textView.TextSnapshot;
            // get all of the previous lines
            var lines = snap.Lines.Reverse().Skip(snap.LineCount - line.LineNumber);
            foreach (ITextSnapshotLine prevLine in lines)
            {
                var text = prevLine.GetText();
                if (text.All(c2 => System.Char.IsWhiteSpace(c2)))
                {
                    continue;
                }
                var toks = Utils.LexString(text).ToList();
                if (toks.Last().Type == RustLexer.RustLexer.LBRACE)
                {
                    return prevLine.GetText().TakeWhile(c2 => c2 == ' ').Count() + 4;
                }
                else if (toks.Any(tok => tok.Type == RustLexer.RustLexer.RBRACE))
                {
                    return prevLine.GetText().TakeWhile(c2 => c2 == ' ').Count();
                }
            }
            // otherwise, there are no lines ending in braces before us.
            return null;
        }

        public void Dispose()
        {
            // why is this required...
        }
    }
}

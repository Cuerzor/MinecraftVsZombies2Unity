#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using MVZ2.Metas;
using MVZ2.TalkData;
using MVZ2Logic.Resources;
using PVZEngine;

public class TalkConverter
{
    // 静态缓存，避免重复创建
    private static readonly string UnknownText = "unknown";
    private static readonly NamespaceID UnknownId = new NamespaceID("mvz2", UnknownText);
    private static readonly NamespaceID[] UnknownIdArray = new[] { UnknownId };

    private readonly TalkConvertContext _context = new TalkConvertContext();
    private readonly TextReader _reader;
    private readonly string _defaultNsp;

    public TalkConverter(TextReader reader, string defaultNsp)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _defaultNsp = defaultNsp ?? throw new ArgumentNullException(nameof(defaultNsp));
    }

    // 暴露上下文（如需要）但保持只读
    public TalkConvertContext Context => _context;
    public TextReader Reader => _reader;
    public string DefaultNsp => _defaultNsp;

    public void Convert(XmlDocument document, XmlNode node)
    {
        string line;
        while ((line = _reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;   // 跳过空行

            ReadLine(line);
        }
        Complete();

        foreach (var pair in _context.SpeakerNames)
        {
            var comment = document.CreateComment($"{pair.Value} = {pair.Key}");
            node.AppendChild(comment);
        }
        foreach (var group in _context.Groups)
        {
            var groupNode = group.ToXmlNode(document);
            node.AppendChild(groupNode);
        }
    }

    public static string GetUnknownText() => UnknownText;
    public static NamespaceID GetUnknownID() => UnknownId;
    public static NamespaceID[] GetUnknownIDArray() => UnknownIdArray;

    private void ReadLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return;
        if (line.StartsWith("#"))
        {
            ReadSectionEnd();
            ReadGroupEnd();
            ReadGroupStart(line);
            return;     // 处理完毕，不再检查其他规则
        }

        if (line.StartsWith("【") || line.StartsWith("["))
        {
            ReadSectionEnd();
            ReadSectionStart(line);
            return;
        }
        if (line.StartsWith("（") || line.StartsWith("("))
        {
            ReadDescription(line);
            return;
        }

        ReadSentence(line);
    }

    private void ReadGroupStart(string line)
    {
        // 去除前导 '#' 和空白即为标题
        string title = line.TrimStart('#', ' ', '\t');
        _context.CurrentGroup = new TalkConvertGroup(title);
    }
    private void ReadGroupEnd()
    {
        _context.GroupEnd(_defaultNsp);
    }

    private void ReadSectionStart(string line)
    {
        int start = -1, end = -1;
        // 查找开括号
        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '[' || line[i] == '【')
            {
                start = i;
                break;
            }
        }
        // 查找对应的闭括号
        for (int i = start + 1; i < line.Length; i++)
        {
            if (line[i] == ']' || line[i] == '】')
            {
                end = i;
                break;
            }
        }

        if (start >= 0 && end > start)
        {
            string desc = line.Substring(start + 1, end - start - 1);
            _context.CurrentSection = new TalkConvertSection()
            {
                Name = desc
            };
        }
        // 若格式不正确，静默忽略
    }

    private void ReadSectionEnd()
    {
        _context.SectionEnd();
    }
    private void Complete()
    {
        ReadSectionEnd();
        ReadGroupEnd();
    }

    private void ReadDescription(string line)
    {
        int start = -1, end = -1;
        // 查找开括号
        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '(' || line[i] == '（')
            {
                start = i;
                break;
            }
        }
        // 查找对应的闭括号
        for (int i = start + 1; i < line.Length; i++)
        {
            if (line[i] == ')' || line[i] == '）')
            {
                end = i;
                break;
            }
        }

        if (start >= 0 && end > start)
        {
            string desc = line.Substring(start + 1, end - start - 1);
            desc = $"（{desc}）";
            if (string.IsNullOrEmpty(_context.CurrentDescription))
                _context.DescriptionStart(desc);
            else
                _context.DescriptionAdd(desc);
        }
        // 若格式不正确，静默忽略
    }

    private void ReadSentence(string line)
    {
        string speaker = string.Empty;
        string? speakerName = null;
        string text = line;

        // 按第一个冒号分割
        int colonIndex = -1;
        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == ':' || line[i] == '：')
            {
                colonIndex = i;
                break;
            }
        }

        if (colonIndex > 0)
        {
            speaker = line.Substring(0, colonIndex).Trim();

            var speakerNameStart = speaker.IndexOfAny(new char[] { '(', '（' });
            var speakerNameEnd = speaker.IndexOfAny(new char[] { ')', '）' });
            if (speakerNameStart >= 0 && speakerNameEnd > speakerNameStart)
            {
                speakerName = speaker.Substring(0, speakerNameStart);
                speaker = speaker.Substring(speakerNameStart + 1, speakerNameEnd - speakerNameStart - 1);
            }

            text = line.Substring(colonIndex + 1).TrimStart();
        }

        var sentence = new TalkConvertSentence(
            _context.AllocSpeakerID(speaker),
            _context.CurrentDescription ?? string.Empty,
            text
        )
        {
            SpeakerName = speakerName
        };

        _context.DescriptionEnd();
        _context.SentenceStart(sentence);
        _context.SentenceEnd();
    }
}

public class TalkConvertContext
{
    public TalkConvertGroup? CurrentGroup { get; set; }
    public TalkConvertSection? CurrentSection { get; set; }
    public TalkConvertSentence? CurrentSentence { get; set; }
    public string? CurrentDescription { get; set; }
    public Dictionary<string, NamespaceID> SpeakerNames { get; set; } = new Dictionary<string, NamespaceID>();
    private int currentSpeakerCount = 0;

    public List<TalkGroup> Groups { get; } = new List<TalkGroup>();

    public void GroupEnd(string defaultNsp)
    {
        if (CurrentGroup != null)
        {
            var group = CurrentGroup.ToGroup(defaultNsp);
            Groups.Add(group);
            CurrentGroup = null;
        }
    }

    public void SectionEnd()
    {
        if (CurrentSection != null)
        {
            var group = GetOrCreateCurrentGroup();
            group.Sections.Add(CurrentSection);
            CurrentSection = null;
        }
    }

    public void SentenceStart(TalkConvertSentence sentence) => CurrentSentence = sentence;

    public void SentenceEnd()
    {
        if (CurrentSentence != null)
        {
            var section = GetOrCreateCurrentSection();
            section.Sentences.Add(CurrentSentence);
            CurrentSentence = null;
        }
    }

    public void DescriptionStart(string description) => CurrentDescription = description;

    public void DescriptionAdd(string description)
    {
        CurrentDescription += $"\\n{description}";
    }

    public void DescriptionEnd() => CurrentDescription = null;

    public NamespaceID AllocSpeakerID(string name)
    {
        if (SpeakerNames.TryGetValue(name, out var newID))
        {
            return newID;
        }
        var id = new NamespaceID("mvz2", $"speaker_{currentSpeakerCount}");
        currentSpeakerCount++;
        SpeakerNames.Add(name, id);
        return id;
    }

    private TalkConvertGroup GetOrCreateCurrentGroup()
    {
        if (CurrentGroup == null)
        {
            CurrentGroup = new TalkConvertGroup(string.Empty);
        }
        return CurrentGroup;
    }

    private TalkConvertSection GetOrCreateCurrentSection()
    {
        if (CurrentSection == null)
        {
            CurrentSection = new TalkConvertSection();
        }
        return CurrentSection;
    }
}

public class TalkConvertGroup
{
    public string Name { get; set; }
    public List<TalkConvertSection> Sections { get; } = new List<TalkConvertSection>();

    public TalkConvertGroup(string name) => Name = name;

    public TalkGroup ToGroup(string defaultNsp)
    {
        var archive = new TalkGroupArchiveInfo
        {
            name = Name,
            background = new SpriteReference(TalkConverter.GetUnknownID()),
            music = TalkConverter.GetUnknownID(),
            unlockConditions = new XMLConditionList(false, new XMLCondition
            {
                Required = TalkConverter.GetUnknownIDArray()
            })
        };

        var sections = Sections.Select(s => s.ToSection(defaultNsp)).ToArray();

        return new TalkGroup(TalkConverter.GetUnknownText(), TalkConverter.GetUnknownIDArray(), sections)
        {
            archive = archive
        };
    }
}

public class TalkConvertSection
{
    public string? Name { get; set; }
    public List<TalkConvertSentence> Sentences { get; } = new List<TalkConvertSentence>();

    public TalkSection ToSection(string defaultNsp)
    {
        var characters = new TalkCharacter[]
        {
            new TalkCharacter(TalkConverter.GetUnknownID(), null, "left")
        };

        var sentences = Sentences.Select((s, i) =>
            s.ToSentence(defaultNsp, i == Sentences.Count - 1)).ToArray();

        return new TalkSection(characters, sentences)
        {
            archiveText = Name ?? string.Empty,
            startScripts = null,
            autoSkipScripts = null,
            skipScripts = null,
            canAutoSkip = true,
        };
    }
}

public class TalkConvertSentence
{
    public NamespaceID Speaker { get; }
    public string? SpeakerName { get; set; }
    public string Description { get; }
    public string Text { get; }

    public TalkConvertSentence(NamespaceID speaker, string description, string text)
    {
        Speaker = speaker;
        Description = description;
        Text = text;
    }

    public TalkSentence ToSentence(string defaultNsp, bool isLast)
    {
        return new TalkSentence(Text, TalkConverter.GetUnknownIDArray())
        {
            clickScripts = isLast ? new TalkScript[] { new TalkScript("end") } : null,
            description = Description,
            speaker = Speaker,
            speakerName = SpeakerName,
        };
    }
}
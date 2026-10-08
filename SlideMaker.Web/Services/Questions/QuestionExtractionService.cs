using System.Text.RegularExpressions;
using SlideMaker.Web.DTOs;

namespace SlideMaker.Web.Services.Questions;

public partial class QuestionExtractionService : IQuestionExtractionService
{
    // "Q1.", "1.", "1)", "Q.1" at the start of a line — the question number line.
    [GeneratedRegex(@"^\s*(?:Q\.?\s*)?(\d{1,3})\s*[\.\):]\s*(.*)$", RegexOptions.IgnoreCase)]
    private static partial Regex QuestionLineRegex();

    // An option marker anywhere on a line — "A.", "A)", or "(a)"/"(८)" (OCR often misreads a
    // parenthesized single-letter label as a lookalike digit/Devanagari character, so the
    // label CHARACTER is not trusted; only its presence/position is — see ParseQuestions,
    // which assigns A/B/C/D by the order options are found, not by what was captured here.
    // Printed papers often lay two options side by side on one physical line ("(a) X   (b) Y"),
    // so this is matched with Regex.Matches (all occurrences), not a single anchored Match.
    [GeneratedRegex(@"(?:\(([^\s\)]{1,2})\)?|([A-Da-d])[\.\):])\s+")]
    private static partial Regex OptionMarkerRegex();

    [GeneratedRegex(@"^\s*(Answer|Ans|Correct Answer|उत्तर|सही उत्तर)\s*[:\-]\s*(.+)$", RegexOptions.IgnoreCase)]
    private static partial Regex AnswerLineRegex();

    [GeneratedRegex(@"^\s*(Explanation|Solution|व्याख्या|स्पष्टीकरण)\s*[:\-]\s*(.*)$", RegexOptions.IgnoreCase)]
    private static partial Regex ExplanationLineRegex();

    public List<ExtractedQuestionDto> ParseQuestions(string rawText)
    {
        var result = new List<ExtractedQuestionDto>();
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return result;
        }

        var lines = rawText.Replace("\r\n", "\n").Split('\n');

        ExtractedQuestionDto? current = null;
        var fallbackNumber = 0;
        var inExplanation = false;

        foreach (var rawLine in lines)
        {
            var line = rawLine.TrimEnd();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var optionMarkers = OptionMarkerRegex().Matches(line);
            var questionMatch = QuestionLineRegex().Match(line);

            if (questionMatch.Success && optionMarkers.Count == 0)
            {
                if (current is not null)
                {
                    result.Add(current);
                }

                fallbackNumber++;
                current = new ExtractedQuestionDto
                {
                    QuestionNumber = int.TryParse(questionMatch.Groups[1].Value, out var num) ? num : fallbackNumber,
                    QuestionText = questionMatch.Groups[2].Value.Trim()
                };
                inExplanation = false;
                continue;
            }

            if (current is null)
            {
                // Nothing looked like a question yet — this is page-header/title noise
                // (e.g. a magazine header, page number) before the first real question.
                continue;
            }

            if (optionMarkers.Count > 0)
            {
                for (var i = 0; i < optionMarkers.Count; i++)
                {
                    var start = optionMarkers[i].Index + optionMarkers[i].Length;
                    var end = i + 1 < optionMarkers.Count ? optionMarkers[i + 1].Index : line.Length;
                    var text = line[start..end].Trim();
                    if (text.Length == 0)
                    {
                        continue;
                    }

                    current.Options.Add(new QuestionOptionDto
                    {
                        Label = ((char)('A' + current.Options.Count)).ToString(),
                        Text = text
                    });
                }
                inExplanation = false;
                continue;
            }

            var answerMatch = AnswerLineRegex().Match(line);
            var explanationMatch = ExplanationLineRegex().Match(line);

            if (answerMatch.Success)
            {
                current.Answer = answerMatch.Groups[2].Value.Trim();
                inExplanation = false;
            }
            else if (explanationMatch.Success)
            {
                current.Explanation = explanationMatch.Groups[2].Value.Trim();
                inExplanation = true;
            }
            else if (inExplanation)
            {
                current.Explanation = string.IsNullOrEmpty(current.Explanation)
                    ? line.Trim()
                    : $"{current.Explanation} {line.Trim()}";
            }
            else if (string.IsNullOrWhiteSpace(current.QuestionText))
            {
                current.QuestionText = line.Trim();
            }
            else
            {
                // Continuation of the question text (wrapped line, no options seen yet).
                if (current.Options.Count == 0)
                {
                    current.QuestionText = $"{current.QuestionText} {line.Trim()}";
                }
            }
        }

        if (current is not null)
        {
            result.Add(current);
        }

        return result;
    }
}

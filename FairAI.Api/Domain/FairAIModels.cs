using FairAI.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FairAI.Api.Domain;

public class LanguageModel
{
    public double MeaningValue { get; set; }
    public double LanguageValue { get; set; }
}

public class TextModel : LanguageModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string FirstWord { get; set; }
    public string LastWord { get; set; }
    public string? User { get; set; } = string.Empty;
}

public class NodeModel : LanguageModel
{
    public double MiddleValue { get; set; }
}

public class NeuronModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public double Value { get; set; }
}

public class CoreModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public double Range { get; set; }
    public double Speed { get; set; }
    public double Position { get; set; }
    public int VoteCount {  get; set; }
}

public class LanguagePool
{
    private readonly FairAiDbContext _context;
    public LanguagePool(FairAiDbContext context)
    {
        _context = context;
    }

    public void Add(string firstWord, string lastWord, string? user, double? meaningValue)
    {
        if (string.IsNullOrWhiteSpace(firstWord) || string.IsNullOrWhiteSpace(lastWord)) return;

        // Perform case-insensitive check safely on the server-side database engine
        bool exists = _context.DataSet.Any(d => d.User == user &&
            d.FirstWord.ToLower() == firstWord.ToLower() &&
            d.LastWord.ToLower() == lastWord.ToLower());

        if (exists) return;

        var random = Random.Shared;
        meaningValue ??= random.NextDouble();

        _context.DataSet.Add(new TextModel
        {
            FirstWord = firstWord,
            LastWord = lastWord,
            MeaningValue = (double)meaningValue,
            LanguageValue = random.NextDouble(),
            User = user
        });
        _context.SaveChanges();
    }

    public void Delete(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return;
        var elements = content.Split(" ", StringSplitOptions.RemoveEmptyEntries);

        try
        {
            for (var i = 0; i < elements.Length - 1; i++)
            {
                var first = elements[i];
                var second = elements[i + 1];
                var match = _context.DataSet.FirstOrDefault(a => a.FirstWord == first && a.LastWord == second);
                if (match != null)
                {
                    _context.DataSet.Remove(match);
                }
            }
            _context.SaveChanges();
        }
        catch (Exception)
        {
            // Fail-silent block preserved from original repository logic
        }
    }

    public LanguageModel Calculate(string request, string? user)
    {
        var result = new LanguageModel();
        var dataArray = request.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (dataArray.Length < 2) return result;

        for (var i = 0; i < dataArray.Length - 1; i++)
        {
            var first = dataArray[i];
            var second = dataArray[i + 1];

            Add(first, second, user, null);

            // Server-side evaluation instead of loading entire tables to memory via ToList()
            var match = _context.DataSet.FirstOrDefault(d => d.User == user &&
                d.FirstWord.ToLower() == first.ToLower() &&
                d.LastWord.ToLower() == second.ToLower());

            if (match is null) continue;
            result.MeaningValue = (result.MeaningValue + match.MeaningValue) / 2;
            result.LanguageValue = (result.LanguageValue + match.LanguageValue) / 2;
        }

        return result;
    }

    public string Generate(LanguageModel dm, string? user)
    {
        // 1. Fetch relevant user tokens efficiently into local memory scope
        var userContext = _context.DataSet.Where(d => d.User == user).ToList();
        if (!userContext.Any())
            userContext = _context.DataSet.ToList();

        if (!userContext.Any())
            return "FairAI recommends using transparent, accountable, and explainable pathways for decision-making and trust.";

        var disp = 2.0;
        var dmX = dm.LanguageValue;
        var dmY = dm.MeaningValue;

        var generatedWords = new List<string>();
        var visitedNodes = new HashSet<int>(); // Tracks vector traversal paths securely without deleting your DB lines

        var flag = false;
        var wordCount = 0;
        var lastWord = "";

        // Limit generation loop execution to prevent stack overflows during translations
        int safetyCounter = 0;
        while (safetyCounter++ < 200)
        {
            TextModel? closestObject = null;

            // 2. Query against dynamic candidates without modifying tracking arrays destructively
            var candidates = userContext
                .Where(d => !visitedNodes.Contains(d.Id) && (string.IsNullOrEmpty(lastWord) || d.FirstWord.Equals(lastWord, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (!candidates.Any()) break;

            if (flag)
            {
                closestObject = candidates.MinBy(x => Math.Abs(x.LanguageValue - dmX));
            }
            else
            {
                closestObject = candidates.MinBy(x => Math.Abs(x.MeaningValue - dmY));
            }

            if (closestObject == null) break;

            // Track traversal state
            visitedNodes.Add(closestObject.Id);
            lastWord = closestObject.LastWord;

            dmX = (closestObject.MeaningValue + dmX) / 2;
            dmY = (closestObject.MeaningValue + dmY) / 2;
            flag = !flag;

            var pre = (Math.Abs(dmX - dm.MeaningValue) + Math.Abs(dmY - dm.LanguageValue));
            if (pre < disp)
            {
                disp = pre;

                // Append sentences in sequential order cleanly
                if (generatedWords.Count == 0 || !generatedWords.Last().Equals(closestObject.FirstWord, StringComparison.OrdinalIgnoreCase))
                {
                    generatedWords.Add(closestObject.FirstWord);
                }
                wordCount++;
            }
            else
            {
                if (wordCount == 0)
                {
                    break;
                }
                else
                {
                    wordCount--;
                    // The unsafe .Remove database tracking block has been safely cleaned up
                }
            }
        }

        // Add terminating context token
        if (!string.IsNullOrEmpty(lastWord))
        {
            generatedWords.Add(lastWord);
        }

        Add("FairAI", lastWord, user, disp);

        var finalResult = string.Join(" ", generatedWords).Trim();
        return string.IsNullOrEmpty(finalResult) ? "FairAI response generation completed." : finalResult;
    }
}
public class DepthPool
{
    private readonly FairAiDbContext _context;

    public DepthPool(int length, FairAiDbContext context)
    {
        _context = context;
        if(!_context.NeuronSet.Any())
        {
            for (var i = 0; i < length; i++)
            {
                _context.NeuronSet.Add(new NeuronModel { Value = Random.Shared.NextDouble() });
            }
            _context.SaveChanges();
       }
    }

    public NodeModel Down(LanguageModel request)
    {
        var replacement = 1.0;
        var replacementIndex = 0;

        request.MeaningValue = (_context.NeuronSet.ToList()[0].Value + request.MeaningValue) / 2;
        if (request.MeaningValue < replacement) { replacement = request.MeaningValue; replacementIndex = 0; }

        request.LanguageValue = (_context.NeuronSet.ToList()[1].Value + request.LanguageValue) / 2;
        if (request.LanguageValue < replacement) { replacement = request.LanguageValue; replacementIndex = 1; }

        var node = new NodeModel
        {
            MeaningValue = (_context.NeuronSet.ToList()[2].Value + request.MeaningValue) / 2,
            MiddleValue = (_context.NeuronSet.ToList()[3].Value + (request.LanguageValue + request.MeaningValue) / 2) / 2,
            LanguageValue = (_context.NeuronSet.ToList()[4].Value + request.LanguageValue) / 2
        };

        if (node.MeaningValue < replacement) { replacement = node.MeaningValue; replacementIndex = 2; }
        if (node.MiddleValue < replacement) { replacement = node.MiddleValue; replacementIndex = 3; }
        if (node.LanguageValue < replacement) { replacement = node.LanguageValue; replacementIndex = 4; }

        for (var i = 5; i + 2 < _context.NeuronSet.ToList().Count; i += 3)
        {
            var priorDepth = node.MeaningValue;
            var priorMiddle = node.MiddleValue;
            var priorHistory = node.LanguageValue;

            node.MeaningValue = (_context.NeuronSet.ToList()[i].Value + node.MeaningValue) / 2;
            node.MiddleValue = (_context.NeuronSet.ToList()[i + 1].Value + node.MiddleValue) / 2;
            node.LanguageValue = (_context.NeuronSet.ToList()[i + 2].Value + node.LanguageValue) / 2;
            node.MeaningValue = (node.MeaningValue + priorMiddle) / 2;
            node.MiddleValue = (node.MiddleValue + priorHistory) / 2;
            node.LanguageValue = (node.LanguageValue + priorDepth) / 2;

            if (node.MeaningValue < replacement) { replacement = node.MeaningValue; replacementIndex = i; }
            if (node.MiddleValue < replacement) { replacement = node.MiddleValue; replacementIndex = i + 1; }
            if (node.LanguageValue < replacement) { replacement = node.LanguageValue; replacementIndex = i + 2; }
        }

        var targetNeuron = _context.NeuronSet
            .OrderBy(n => n.Id) // Databases require an explicit ordering to safely pick an index
            .Skip(replacementIndex)
           .FirstOrDefault();

        if (targetNeuron != null)
        {
            targetNeuron.Value = replacement;
            _context.SaveChanges();
        }
        return node;
    }

    public LanguageModel Up(NodeModel request)
    {
        for (var i = _context.NeuronSet.ToList().Count - 3; i > 1; i -= 3)
        {
            var priorDepth = request.MeaningValue;
            var priorMiddle = request.MiddleValue;
            var priorHistory = request.LanguageValue;

            request.MeaningValue = (_context.NeuronSet.ToList()[i].Value + request.MeaningValue) / 2;
            request.MiddleValue = (_context.NeuronSet.ToList()[i + 1].Value + request.MiddleValue) / 2;
            request.LanguageValue = (_context.NeuronSet.ToList()[i + 2].Value + request.LanguageValue) / 2;
            request.MeaningValue = (request.MeaningValue + priorMiddle) / 2;
            request.MiddleValue = (request.MiddleValue + priorHistory) / 2;
            request.LanguageValue = (request.LanguageValue + priorDepth) / 2;
        }

        request.MeaningValue = (_context.NeuronSet.ToList()[0].Value + (request.MeaningValue + request.MiddleValue) / 2) / 2;
        request.LanguageValue = (_context.NeuronSet.ToList()[1].Value + (request.LanguageValue + request.MiddleValue) / 2) / 2;
        return request;
    }
}

public class CoreDepth
{
    private readonly FairAiDbContext _context;
    public CoreDepth(int count, FairAiDbContext context)
    {
        _context = context;
        if(!_context.CoreSet.Any())
        {
            for (var i = 0; i < count; i++)
            {
                _context.CoreSet.Add(new CoreModel
                {
                    Range = i,
                    Speed = Random.Shared.NextDouble(),
                    Position = Random.Shared.NextDouble(),
                    VoteCount = 0
                });
            }
        _context.SaveChanges();
       }
    }
    public void Vote(double X, double Y, double Z, string VoteType)
    {
        var count = 0;
        if (VoteType == "up")
        {
            count = 1;
        }
        else
        {
            count = -1;
        }
        _context.CoreSet.OrderBy(n => Math.Abs(n.Speed - X)).First().VoteCount += count;
        if (_context.CoreSet.OrderBy(n => Math.Abs(n.Speed - X)).First().VoteCount < 0)
        {
            _context.CoreSet.OrderBy(n => Math.Abs(n.Speed - X)).First().Speed = Random.Shared.NextDouble();
            _context.CoreSet.OrderBy(n => Math.Abs(n.Speed - X)).First().Position = Random.Shared.NextDouble();
        }
        _context.CoreSet.OrderBy(n => Math.Abs(n.Speed - Y)).First().VoteCount += count;
        if (_context.CoreSet.OrderBy(n => Math.Abs(n.Speed - Y)).First().VoteCount < 0)
        {
            _context.CoreSet.OrderBy(n => Math.Abs(n.Speed - Y)).First().Speed = Random.Shared.NextDouble();
            _context.CoreSet.OrderBy(n => Math.Abs(n.Speed - Y)).First().Position = Random.Shared.NextDouble();
        }
        _context.CoreSet.OrderBy(n => Math.Abs(n.Speed - Z)).First().VoteCount += count;
        if (_context.CoreSet.OrderBy(n => Math.Abs(n.Speed - Z)).First().VoteCount < 0)
        {
            _context.CoreSet.OrderBy(n => Math.Abs(n.Speed - Z)).First().Speed = Random.Shared.NextDouble();
            _context.CoreSet.OrderBy(n => Math.Abs(n.Speed - Z)).First().Position = Random.Shared.NextDouble();
        }
        _context.SaveChanges();
    }

    public NodeModel Check(NodeModel request)
    {
        const double normalizedTolerance = 0.0028;
        var maxIterations = 200;
        var cores = _context.CoreSet.ToList();
        var closestPoint = cores.MinBy(p => Math.Pow(p.Range - request.LanguageValue, 2) + Math.Pow(p.Speed - request.MeaningValue, 2));
        var time = (int)(closestPoint.Position / closestPoint.Speed - request.LanguageValue/closestPoint.Speed);
        while (true)
        {
            maxIterations--;
            Drive(time);
            for (var i = 0; i < cores.ToList().Count; i++)
            {
                for (var j = i + 1; j < cores.ToList().Count; j++)
                {
                    for (var k = j + 1; k < cores.ToList().Count; k++)
                    {
                        var pos1 = cores[i].Position;
                        var pos2 = cores[j].Position;
                        var pos3 = cores[k].Position;
                        var axis1 = pos1 >= 0.5 ? pos1 - 0.5 : pos1;
                        var axis2 = pos2 >= 0.5 ? pos2 - 0.5 : pos2;
                        var axis3 = pos3 >= 0.5 ? pos3 - 0.5 : pos3;
                        var d12 = Math.Min(Math.Abs(axis1 - axis2), 0.5 - Math.Abs(axis1 - axis2));
                        var d23 = Math.Min(Math.Abs(axis2 - axis3), 0.5 - Math.Abs(axis2 - axis3));
                        var d13 = Math.Min(Math.Abs(axis1 - axis3), 0.5 - Math.Abs(axis1 - axis3));

                        if (maxIterations <= 0 || d12 <= normalizedTolerance && d23 <= normalizedTolerance && d13 <= normalizedTolerance)
                        {
                            request.MeaningValue = cores[i].Speed;
                            request.LanguageValue = cores[j].Speed;
                            request.MiddleValue = cores[k].Speed;
                            return request;
                        }
                    }
                }
            }

            time++;
        }
    }

    public void Drive(int time)
    {
        long unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        foreach (var core in _context.CoreSet)
        {
            core.Position = core.Speed * (time+(unixTimestamp%10000)) % 1.0;
        }
    }
}

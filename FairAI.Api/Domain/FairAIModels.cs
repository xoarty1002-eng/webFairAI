using System.ComponentModel.DataAnnotations;
using FairAI.Api.Data;

namespace FairAI.Api.Domain;

public class StateModel
{
    [Key]
    public int Id { get; set; }
    public double DepthValue { get; set; }
    public double HistoryValue { get; set; }
}

public class DataModel : StateModel
{
    public string Word { get; set; } = string.Empty;
    public string? User { get; set; } = string.Empty;
}

public class NodeModel : StateModel
{
    public double MiddleValue { get; set; }
}

public class NeuronModel
{
    [Key]
    public int Id { get; set; }
    public double Value { get; set; }
}

public class CoreModel
{
    [Key]
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

    public void Add(string word, string? user)
    {
        if (string.IsNullOrWhiteSpace(word)) return;

        if (_context.DataSet.ToList().Any(d => d.User == user && string.Equals(d.Word, word, StringComparison.OrdinalIgnoreCase))) return;

        var random = Random.Shared;
        _context.DataSet.Add(new DataModel
        {
            Word = word,
            DepthValue = random.NextDouble(),
            HistoryValue = random.NextDouble(),
            User = user
        });
        _context.SaveChanges();

    }

    public StateModel Calculate(string request, string? user)
    {
        var result = new StateModel();
        var dataArray = request.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var element in dataArray)
        {
            Add(element, user);
            var match = _context.DataSet.ToList().FirstOrDefault(d => d.User == user && string.Equals(d.Word, element, StringComparison.OrdinalIgnoreCase));
            if (match is null) continue;
            result.DepthValue = (result.DepthValue + match.DepthValue) / 2;
            result.HistoryValue = (result.HistoryValue + match.HistoryValue) / 2;
        }

        return result;
    }

    public string Generate(StateModel dm, string? user)
    {
        if (! _context.DataSet.ToList().Where(d => d.User == user).ToList().Any()) 
        return "FairAI recommends using transparent, accountable, and explainable pathways for decision-making and trust.";
        var disp = 2.0;
        var dmX = dm.HistoryValue;
        var dmY = dm.DepthValue;
        var str = "";
        DataModel closestObject;
        var flag = true;
        var wordCount = 0;
        while (true)
        {
            if (flag)
            {
                closestObject =  _context.DataSet.ToList().Where(d => d.User == user).ToList().MinBy(x =>
                    Math.Abs(x.HistoryValue - dmX)
            );
            }
            else
            {
                closestObject =  _context.DataSet.ToList().Where(d => d.User == user).ToList().MinBy(x =>
                Math.Abs(x.DepthValue - dmY)
            );
            }
            dmX = (closestObject.DepthValue + dmX) / 2;
            dmY = (closestObject.DepthValue + dmY) / 2;
            flag = !flag;
            var pre = (Math.Abs(dmX - dm.DepthValue) + Math.Abs(dmY - dm.HistoryValue));
            if (pre < disp)
            {
                disp = pre;
                str += closestObject.Word + " ";
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
                    _context.DataSet.Remove(closestObject);
                    _context.SaveChanges();
                    Add(closestObject.Word, user);
                }
            }
        }
        return str;
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

    public NodeModel Down(StateModel request)
    {
        var replacement = 1.0;
        var replacementIndex = 0;

        request.DepthValue = (_context.NeuronSet.ToList()[0].Value + request.DepthValue) / 2;
        if (request.DepthValue < replacement) { replacement = request.DepthValue; replacementIndex = 0; }

        request.HistoryValue = (_context.NeuronSet.ToList()[1].Value + request.HistoryValue) / 2;
        if (request.HistoryValue < replacement) { replacement = request.HistoryValue; replacementIndex = 1; }

        var node = new NodeModel
        {
            DepthValue = (_context.NeuronSet.ToList()[2].Value + request.DepthValue) / 2,
            MiddleValue = (_context.NeuronSet.ToList()[3].Value + (request.HistoryValue + request.DepthValue) / 2) / 2,
            HistoryValue = (_context.NeuronSet.ToList()[4].Value + request.HistoryValue) / 2
        };

        if (node.DepthValue < replacement) { replacement = node.DepthValue; replacementIndex = 2; }
        if (node.MiddleValue < replacement) { replacement = node.MiddleValue; replacementIndex = 3; }
        if (node.HistoryValue < replacement) { replacement = node.HistoryValue; replacementIndex = 4; }

        for (var i = 5; i + 2 < _context.NeuronSet.ToList().Count; i += 3)
        {
            var priorDepth = node.DepthValue;
            var priorMiddle = node.MiddleValue;
            var priorHistory = node.HistoryValue;

            node.DepthValue = (_context.NeuronSet.ToList()[i].Value + node.DepthValue) / 2;
            node.MiddleValue = (_context.NeuronSet.ToList()[i + 1].Value + node.MiddleValue) / 2;
            node.HistoryValue = (_context.NeuronSet.ToList()[i + 2].Value + node.HistoryValue) / 2;
            node.DepthValue = (node.DepthValue + priorMiddle) / 2;
            node.MiddleValue = (node.MiddleValue + priorHistory) / 2;
            node.HistoryValue = (node.HistoryValue + priorDepth) / 2;

            if (node.DepthValue < replacement) { replacement = node.DepthValue; replacementIndex = i; }
            if (node.MiddleValue < replacement) { replacement = node.MiddleValue; replacementIndex = i + 1; }
            if (node.HistoryValue < replacement) { replacement = node.HistoryValue; replacementIndex = i + 2; }
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

    public StateModel Up(NodeModel request)
    {
        for (var i = _context.NeuronSet.ToList().Count - 3; i > 1; i -= 3)
        {
            var priorDepth = request.DepthValue;
            var priorMiddle = request.MiddleValue;
            var priorHistory = request.HistoryValue;

            request.DepthValue = (_context.NeuronSet.ToList()[i].Value + request.DepthValue) / 2;
            request.MiddleValue = (_context.NeuronSet.ToList()[i + 1].Value + request.MiddleValue) / 2;
            request.HistoryValue = (_context.NeuronSet.ToList()[i + 2].Value + request.HistoryValue) / 2;
            request.DepthValue = (request.DepthValue + priorMiddle) / 2;
            request.MiddleValue = (request.MiddleValue + priorHistory) / 2;
            request.HistoryValue = (request.HistoryValue + priorDepth) / 2;
        }

        request.DepthValue = (_context.NeuronSet.ToList()[0].Value + (request.DepthValue + request.MiddleValue) / 2) / 2;
        request.HistoryValue = (_context.NeuronSet.ToList()[1].Value + (request.HistoryValue + request.MiddleValue) / 2) / 2;
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
        var time = 1;
        var maxIterations = 200;
        var cores = _context.CoreSet.ToList();
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
                            request.DepthValue =cores[i].Speed;
                            request.HistoryValue = cores[j].Speed;
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

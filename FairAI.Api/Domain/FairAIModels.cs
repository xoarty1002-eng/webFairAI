namespace FairAI.Api.Domain;

public class StateModel
{
    public double DepthValue { get; set; }
    public double HistoryValue { get; set; }
}

public class DataModel : StateModel
{
    public string Word { get; set; } = string.Empty;
}

public class NodeModel : StateModel
{
    public double MiddleValue { get; set; }
}

public class NeuronModel
{
    public double Value { get; set; }
}

public class CoreModel
{
    public double Range { get; set; }
    public double Speed { get; set; }
    public double Position { get; set; }
}

public class LanguagePool
{
    public List<DataModel> Data { get; set; } = new();

    public void Add(string word)
    {
        if (string.IsNullOrWhiteSpace(word)) return;

        if (Data.Any(d => string.Equals(d.Word, word, StringComparison.OrdinalIgnoreCase))) return;

        var random = Random.Shared;
        Data.Add(new DataModel
        {
            Word = word,
            DepthValue = random.NextDouble(),
            HistoryValue = random.NextDouble()
        });
    }

    public StateModel Calculate(string request)
    {
        var result = new StateModel();
        var dataArray = request.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var element in dataArray)
        {
            Add(element);
            var match = Data.FirstOrDefault(d => string.Equals(d.Word, element, StringComparison.OrdinalIgnoreCase));
            if (match is null) continue;
            result.DepthValue = (result.DepthValue + match.DepthValue) / 2;
            result.HistoryValue = (result.HistoryValue + match.HistoryValue) / 2;
        }

        return result;
    }

    public string Generate(StateModel state)
    {
        if (Data.Count == 0) return string.Empty;

        double targetFirst = state.HistoryValue;
        double targetSecond = state.DepthValue;
        var generated = new List<string>();
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var toggle = true;

        while (generated.Count < Data.Count)
        {
            DataModel? candidate = null;

            if (toggle)
            {
                candidate = Data
                    .Where(d => !visited.Contains(d.Word))
                    .OrderBy(d => Math.Abs(d.HistoryValue - targetFirst))
                    .FirstOrDefault();

                if (candidate is null) break;

                var delta = targetFirst - candidate.HistoryValue;
                targetSecond += delta;
            }
            else
            {
                candidate = Data
                    .Where(d => !visited.Contains(d.Word))
                    .OrderBy(d => Math.Abs(d.DepthValue - targetSecond))
                    .FirstOrDefault();

                if (candidate is null) break;

                var delta = targetSecond - candidate.DepthValue;
                targetFirst += delta;
            }

            if (candidate is null) break;

            visited.Add(candidate.Word);
            generated.Add(candidate.Word);
            toggle = !toggle;
        }

        return string.Join(" ", generated).Trim();
    }
}

public class DepthPool
{
    public List<NeuronModel> Pool { get; set; }

    public DepthPool(int length)
    {
        Pool = new List<NeuronModel>();
        for (var i = 0; i < length; i++)
        {
            Pool.Add(new NeuronModel { Value = Random.Shared.NextDouble() });
        }
    }

    public NodeModel Down(StateModel request)
    {
        var replacement = 1.0;
        var replacementIndex = 0;

        request.DepthValue = (Pool[0].Value + request.DepthValue) / 2;
        if (request.DepthValue < replacement) { replacement = request.DepthValue; replacementIndex = 0; }

        request.HistoryValue = (Pool[1].Value + request.HistoryValue) / 2;
        if (request.HistoryValue < replacement) { replacement = request.HistoryValue; replacementIndex = 1; }

        var node = new NodeModel
        {
            DepthValue = (Pool[2].Value + request.DepthValue) / 2,
            MiddleValue = (Pool[3].Value + (request.HistoryValue + request.DepthValue) / 2) / 2,
            HistoryValue = (Pool[4].Value + request.HistoryValue) / 2
        };

        if (node.DepthValue < replacement) { replacement = node.DepthValue; replacementIndex = 2; }
        if (node.MiddleValue < replacement) { replacement = node.MiddleValue; replacementIndex = 3; }
        if (node.HistoryValue < replacement) { replacement = node.HistoryValue; replacementIndex = 4; }

        for (var i = 5; i + 2 < Pool.Count; i += 3)
        {
            var priorDepth = node.DepthValue;
            var priorMiddle = node.MiddleValue;
            var priorHistory = node.HistoryValue;

            node.DepthValue = (Pool[i].Value + node.DepthValue) / 2;
            node.MiddleValue = (Pool[i + 1].Value + node.MiddleValue) / 2;
            node.HistoryValue = (Pool[i + 2].Value + node.HistoryValue) / 2;
            node.DepthValue = (node.DepthValue + priorMiddle) / 2;
            node.MiddleValue = (node.MiddleValue + priorHistory) / 2;
            node.HistoryValue = (node.HistoryValue + priorDepth) / 2;

            if (node.DepthValue < replacement) { replacement = node.DepthValue; replacementIndex = i; }
            if (node.MiddleValue < replacement) { replacement = node.MiddleValue; replacementIndex = i + 1; }
            if (node.HistoryValue < replacement) { replacement = node.HistoryValue; replacementIndex = i + 2; }
        }

        Pool[replacementIndex].Value = replacement;
        return node;
    }

    public StateModel Up(NodeModel request)
    {
        for (var i = Pool.Count - 3; i > 1; i -= 3)
        {
            var priorDepth = request.DepthValue;
            var priorMiddle = request.MiddleValue;
            var priorHistory = request.HistoryValue;

            request.DepthValue = (Pool[i].Value + request.DepthValue) / 2;
            request.MiddleValue = (Pool[i + 1].Value + request.MiddleValue) / 2;
            request.HistoryValue = (Pool[i + 2].Value + request.HistoryValue) / 2;
            request.DepthValue = (request.DepthValue + priorMiddle) / 2;
            request.MiddleValue = (request.MiddleValue + priorHistory) / 2;
            request.HistoryValue = (request.HistoryValue + priorDepth) / 2;
        }

        request.DepthValue = (Pool[0].Value + (request.DepthValue + request.MiddleValue) / 2) / 2;
        request.HistoryValue = (Pool[1].Value + (request.HistoryValue + request.MiddleValue) / 2) / 2;
        return request;
    }
}

public class CoreDepth
{
    public List<CoreModel> Cores { get; set; }

    public CoreDepth(int count)
    {
        Cores = new List<CoreModel>();
        for (var i = 0; i < count; i++)
        {
            Cores.Add(new CoreModel
            {
                Range = i,
                Speed = Random.Shared.NextDouble(),
                Position = Random.Shared.NextDouble()
            });
        }
    }

    public NodeModel Check(NodeModel request)
    {
        const double normalizedTolerance = 0.0028;
        var time = 1;

        while (true)
        {
            Drive(time);
            for (var i = 0; i < Cores.Count; i++)
            {
                for (var j = i + 1; j < Cores.Count; j++)
                {
                    for (var k = j + 1; k < Cores.Count; k++)
                    {
                        var pos1 = Cores[i].Position;
                        var pos2 = Cores[j].Position;
                        var pos3 = Cores[k].Position;
                        var axis1 = pos1 >= 0.5 ? pos1 - 0.5 : pos1;
                        var axis2 = pos2 >= 0.5 ? pos2 - 0.5 : pos2;
                        var axis3 = pos3 >= 0.5 ? pos3 - 0.5 : pos3;
                        var d12 = Math.Min(Math.Abs(axis1 - axis2), 0.5 - Math.Abs(axis1 - axis2));
                        var d23 = Math.Min(Math.Abs(axis2 - axis3), 0.5 - Math.Abs(axis2 - axis3));
                        var d13 = Math.Min(Math.Abs(axis1 - axis3), 0.5 - Math.Abs(axis1 - axis3));

                        if (d12 <= normalizedTolerance && d23 <= normalizedTolerance && d13 <= normalizedTolerance)
                        {
                            request.DepthValue = Cores[i].Speed;
                            request.HistoryValue = Cores[j].Speed;
                            request.MiddleValue = Cores[k].Speed;
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
        foreach (var core in Cores)
        {
            core.Position = (core.Speed * time) % 1.0;
        }
    }
}

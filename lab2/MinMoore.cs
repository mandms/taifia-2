using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace lab2
{
    public class MinMoore
    {
        private Dictionary<string, string> _statesOutputsPairs = new Dictionary<string, string>();
        private List<List<string>> _transactions = new List<List<string>>();
        List<string> _states = new List<string>();
        private List<string> _input = new List<string>();
        private string _inputFilename;
        private string _outputFilename;

        public MinMoore(string inputFilename, string outputFilename)
        {
            _inputFilename = inputFilename;
            _outputFilename = outputFilename;
            ReadFile();
            Dictionary<int, bool> visited = new Dictionary<int, bool>();
            for (int i = 0; i < _states.Count; i++)
            {
                visited.Add(i, false);
            }
            int currentVertex = 0;
            RemoveUnreachableStates(visited, currentVertex);
            for (int i = 0; i < visited.Count; i++)
            {
                if (!visited[i])
                {
                    _statesOutputsPairs.Remove(_states[i]);
                }
            }
            foreach (var item in visited)
            {
                if (!item.Value)
                {
                    foreach (var transaction in _transactions)
                    {
                        transaction[item.Key] = "-";
                    }
                    _states[item.Key] = "-";
                }
            }

            _states.RemoveAll(x => x == "-");
            _transactions.ForEach(x => x.RemoveAll(v => v == "-"));
        }

        private void RemoveUnreachableStates(Dictionary<int, bool> visited, int currentVertex)
        {
            visited[currentVertex] = true;
            for (int i = 0; i < _transactions.Count; i++)
            {
                if (!visited[_states.FindIndex(x => x == _transactions[i][currentVertex])])
                {
                    RemoveUnreachableStates(visited, _states.FindIndex(x => x == _transactions[i][currentVertex]));
                }
            }
        }

        private void ReadFile()
        {
            string[] lines = File.ReadAllLines(_inputFilename);

            string[] outputData = lines[0].Split(';').Skip(1).ToArray();
            string[] states = lines[1].Split(';').Skip(1).ToArray();

            for (int i = 0; i < outputData.Length; i++)
            {
                if (!string.IsNullOrEmpty(states[i]) && i < outputData.Length)
                {
                    _statesOutputsPairs[states[i]] = outputData[i];
                }
            }

            _states.AddRange(_statesOutputsPairs.Keys);

            foreach (string line in lines.Skip(2))
            {
                string[] splitLines = line.Split(';');
                _input.Add(splitLines[0]);
                _transactions.Add(splitLines.Skip(1).ToList());
            }
        }

        int Find(List<List<string>> data, string column, List<string> state)
        {
            int idx = -1;
            for (int i = 0; i < data.Count; i++)
            {
                idx = data[i].FindIndex(x => x == column);
                if (idx >= 0 && state.Contains(data[i][1]))
                {
                    return i;
                }
            }
            return idx;
        }

        private List<List<string>> SortByOutputValues()
        {
            List<List<string>> data = new List<List<string>>();
            _statesOutputsPairs = _statesOutputsPairs.OrderBy(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
            string state = "";
            List<string> output = new List<string>();
            foreach (var pair in _statesOutputsPairs)
            {
                if (pair.Value != state)
                {
                    state = pair.Value;
                    output = new List<string>() { pair.Value, pair.Key };
                    data.Add(output);
                    continue;
                }
                data.Last().Add(pair.Key);
            }
            return data;
        }

        private List<List<string>> ClassDivider(List<List<string>> states, List<List<string>> newTransactions)
        {
            List<List<string>> tmp = new List<List<string>>();

            foreach (var state in states)
            {
                for (int i = 1; i < state.Count; i++)
                {
                    string stateName = state[i];
                    int idx = _states.IndexOf(stateName);
                    string column = "";

                    foreach (var transaction in newTransactions)
                    {
                        if (_statesOutputsPairs.Keys.Contains(transaction[idx]))
                        {
                            column += _statesOutputsPairs[transaction[idx]];
                            continue;
                        }
                        column += transaction[idx];
                    }
                    int clssId = Find(tmp, column, state);
                    if ((clssId != -1))
                    {
                        tmp[clssId].Add(stateName);
                    }
                    else
                    {
                        tmp.Add(new List<string>() { column, stateName });
                    }
                }
            }

            return tmp;
        }

        public List<List<string>> CreateNewTable(List<List<string>> states, List<List<string>> newTransactions)
        {
            for (int i = 0; i < _transactions.Count; i++)
            {
                var transaction = _transactions[i];
                for (int j = 0; j < transaction.Count; j++)
                {
                    var value = transaction[j];
                    foreach (var item in states)
                    {
                        if (item.Contains(value))
                        {
                            newTransactions[i][j] = item[0];
                        }
                    }
                }
            }

            return newTransactions;
        }

        private void Write(List<List<string>> states)
        {

            List<List<string>> newTable = new List<List<string>>();

            for (int i = 0; i < states.Count; i++)
            {
                states[i][0] = "C" + i;
            }

            foreach (var stateName in states)
            {
                int stateIdx = _states.FindIndex(s => s == stateName[1]);
                List<string> tmp = new List<string>();
                for (int i = 0; i < _transactions.Count; i++)
                {
                    string state = _transactions[i][stateIdx];
                    foreach (var item in states)
                    {
                        if (item.Contains(state))
                        {
                            tmp.Add(item[0]);
                        }
                    }

                }
                newTable.Add(tmp);
            }

            ClearFile();

            foreach (var item in states)
            {
                File.AppendAllText(_outputFilename, ";" + _statesOutputsPairs[item[1]]);
            }
            File.AppendAllText(_outputFilename, "\n");
            foreach (var item in states)
            {
                File.AppendAllText(_outputFilename, ";" + item[0]);
            }
            File.AppendAllText(_outputFilename, "\n");
            for (int i = 0; i <= newTable[0].Count - 1; i++)
            {
                File.AppendAllText(_outputFilename, _input[i]);
                foreach (var row in newTable)
                {
                    File.AppendAllText(_outputFilename, ";" + row[i]);
                }
                File.AppendAllText(_outputFilename, "\n");
            }
        }

        void ClearFile() => File.WriteAllText(_outputFilename, string.Empty);

        public void Minimize()
        {
            List<List<string>> newStates = new List<List<string>>();
            newStates = SortByOutputValues();

            List<List<string>> newTransactions = new List<List<string>>();
            newTransactions = _transactions
            .Select(innerList => new List<string>(innerList))
            .ToList();
            int statesCount = 0;

            do
            {
                statesCount = newStates.Count.Equals(0) ? 0 : newStates.Count;
                newStates = ClassDivider(newStates, newTransactions);
                newTransactions = CreateNewTable(newStates, newTransactions);
            } while (statesCount != newStates.Count());

            Write(newStates);
        }
    }
}

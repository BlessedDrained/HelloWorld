namespace HelloWorld.Leetcode;

public class TwoSumSolution
{
    /// <summary>
    /// Сложность: O(n) по времени, O(n) по памяти.
    /// Однопроходной перебор массива с хешированием. Dictionary.TryGetValue и присваивание выполняются за O(1) в среднем,
    /// поэтому полный цикл из n итераций даёт O(n). Пространство — словарь размером до n элементов.
    ///
    /// Время выполнения на Leetcode: 1ms
    /// </summary>
    public int[] TwoSum(int[] nums, int target) {
        var cache = new Dictionary<int, int>();
        for (var i = 0; i < nums.Length; i++)
        {
            var diff = target - nums[i];
            if (cache.TryGetValue(diff, out var index))
            {
                return [i, index];
            }

            cache[nums[i]] = i;
        }

        return [];
    }

    /// <summary>
    /// Сложность: O(n^2) по времени, O(1) по памяти.
    /// Двойной вложенный цикл проверяет все пары элементов. Внешний цикл — n итераций, внутренний — до n-1,
    /// итого O(n^2). Пространство — константное, без дополнительных структур.
    ///
    /// Время выполнения на Leetcode: 62ms
    /// </summary>
    public int[] TwoSumBruteForce(int[] nums, int target) {
        for (var i = 0; i < nums.Length; i++)
        {
            for (var j = i + 1; j < nums.Length; j++)
            {
                if (nums[i] + nums[j] == target)
                {
                    return [i, j];
                }
            }
        }

        return [];
    }
}
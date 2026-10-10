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
}
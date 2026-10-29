namespace HelloWorld.Leetcode;

public class ContainsDuplicateSolution
{
    /// <summary>
    /// Сложность: O(n) по времени, O(n) по памяти.
    /// ToHashSet() проходит весь массив из n элементов, каждая вставка в HashSet выполняется за O(1) в среднем,
    /// поэтому суммарно O(n). Сравнение Count с Length — O(1). Пространство — HashSet размером до n элементов.
    ///
    /// Время выполнения на Leetcode: 16ms
    /// </summary>
    public bool ContainsDuplicate(int[] nums)
    {
        var count = nums.ToHashSet().Count;
        return count != nums.Length;
    }
}
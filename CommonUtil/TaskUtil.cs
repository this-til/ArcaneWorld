using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CommonUtil;

public class TaskUtil {

    /// <summary>
    /// 批量并行处理集合 (Niger给看批)
    /// 将工作分配到固定数量的线程上，每个线程连续处理多个项目
    /// </summary>
    public static Task ParallelProcessBatch<T>(IReadOnlyList<T> items, Action<T> action) {
        int processorCount = Environment.ProcessorCount;
        int batchSize = Math.Max(1, items.Count / processorCount);

        return Task.WhenAll(
            Enumerable.Range(0, processorCount)
                .Select(
                    threadIndex => Task.Run(
                        () => {
                            int start = threadIndex * batchSize;
                            int end = threadIndex == processorCount - 1
                                ? items.Count
                                : Math.Min(start + batchSize, items.Count);

                            for (int i = start; i < end; i++) {
                                action(items[i]);
                            }
                        }
                    )
                )
        );
    }

    /// <summary>
    /// 批量并行处理集合 - 异步版本
    /// 将工作分配到固定数量的线程上，每个线程连续处理多个项目
    /// </summary>
    public static Task ParallelProcessBatch<T>(IReadOnlyList<T> items, Func<T, Task> asyncAction) {
        int processorCount = Environment.ProcessorCount;
        int batchSize = Math.Max(1, items.Count / processorCount);

        return Task.WhenAll(
            Enumerable.Range(0, processorCount)
                .Select(
                    threadIndex => Task.Run(
                        async () => {
                            int start = threadIndex * batchSize;
                            int end = threadIndex == processorCount - 1
                                ? items.Count
                                : Math.Min(start + batchSize, items.Count);

                            for (int i = start; i < end; i++) {
                                await asyncAction(items[i]);
                            }
                        }
                    )
                )
        );
    }

    /// <summary>
    /// 批量并行处理集合，带索引参数
    /// 将工作分配到固定数量的线程上，每个线程连续处理多个项目
    /// </summary>
    public static Task ParallelProcessBatch<T>(IReadOnlyList<T> items, Action<T, int> action) {
        int processorCount = Environment.ProcessorCount;
        int batchSize = Math.Max(1, items.Count / processorCount);

        return Task.WhenAll(
            Enumerable.Range(0, processorCount)
                .Select(
                    threadIndex => Task.Run(
                        () => {
                            int start = threadIndex * batchSize;
                            int end = threadIndex == processorCount - 1
                                ? items.Count
                                : Math.Min(start + batchSize, items.Count);

                            for (int i = start; i < end; i++) {
                                action(items[i], i);
                            }
                        }
                    )
                )
        );
    }

    /// <summary>
    /// 批量并行处理集合，带索引参数 - 异步版本
    /// 将工作分配到固定数量的线程上，每个线程连续处理多个项目
    /// </summary>
    public static Task ParallelProcessBatch<T>(IReadOnlyList<T> items, Func<T, int, Task> asyncAction) {
        int processorCount = Environment.ProcessorCount;
        int batchSize = Math.Max(1, items.Count / processorCount);

        return Task.WhenAll(
            Enumerable.Range(0, processorCount)
                .Select(
                    threadIndex => Task.Run(
                        async () => {
                            int start = threadIndex * batchSize;
                            int end = threadIndex == processorCount - 1
                                ? items.Count
                                : Math.Min(start + batchSize, items.Count);

                            for (int i = start; i < end; i++) {
                                await asyncAction(items[i], i);
                            }
                        }
                    )
                )
        );
    }

}

using BenchmarkDotNet.Attributes;
using Evydencia.PhotoSelector.Application.Display;
using Evydencia.PhotoSelector.Imaging.Sizing;

namespace Evydencia.PhotoSelector.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(launchCount: 1, warmupCount: 1, iterationCount: 3)]
public class DecodeTargetCalculatorBenchmarks
{
    private readonly DecodeTargetCalculator _calculator = new();
    private readonly DisplayContextSnapshot _displayContext = new(
        "benchmark-display",
        effectiveWidthDips: 1920,
        effectiveHeightDips: 1080,
        viewerUsableWidthDips: 1920,
        viewerUsableHeightDips: 1080,
        rasterizationScale: 1.5,
        isFullscreen: true,
        DisplayRole.Customer);

    [Params(1, 6)]
    public int ExifOrientation { get; set; }

    [Benchmark]
    public DecodeTarget Calculate24MpFitTarget()
    {
        return _calculator.Calculate(new DecodeTargetRequest(
            originalWidth: 6000,
            originalHeight: 4000,
            exifOrientation: ExifOrientation,
            displayContext: _displayContext));
    }
}

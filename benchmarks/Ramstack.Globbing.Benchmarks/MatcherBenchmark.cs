namespace Ramstack.Globbing.Benchmarks;

[OperationsPerSecond]
public class MatcherBenchmark
{
    [Benchmark(Description = "literal: a")]
    public bool Literal_1() =>
        Matcher.IsMatch("a", "a", MatchFlags.Unix);

    // [Benchmark(Description = "literal: src")]
    // public bool Literal_2() =>
    //     Matcher.IsMatch("src", "src", MatchFlags.Unix);
    //
    // [Benchmark(Description = "literal: source")]
    // public bool Literal_3() =>
    //     Matcher.IsMatch("source", "source", MatchFlags.Unix);
    //
    // [Benchmark(Description = "literal: password_generation")]
    // public bool Literal_4() =>
    //     Matcher.IsMatch("password_generation", "password_generation", MatchFlags.Unix);
    //
    // [Benchmark(Description = "star: * -> a")]
    // public bool Star_1() =>
    //     Matcher.IsMatch("a", "*", MatchFlags.Unix);
    //
    // [Benchmark(Description = "star: * -> source")]
    // public bool Star_2() =>
    //     Matcher.IsMatch("source", "*", MatchFlags.Unix);
    //
    // [Benchmark(Description = "star: * -> password_generation")]
    // public bool Star_3() =>
    //     Matcher.IsMatch("password_generation", "*", MatchFlags.Unix);
    //
    // [Benchmark(Description = "star: *generation -> password_generation")]
    // public bool Star_4() =>
    //     Matcher.IsMatch("password_generation", "*generation", MatchFlags.Unix);
    //
    // [Benchmark(Description = "star: password* -> password_generation")]
    // public bool Star_5() =>
    //     Matcher.IsMatch("password_generation", "password*", MatchFlags.Unix);
    //
    // [Benchmark(Description = "star: password*generation -> password_generation")]
    // public bool Star_6() =>
    //     Matcher.IsMatch("password_generation", "password*generation", MatchFlags.Unix);
    //
    // [Benchmark(Description = "star: *.generated.* -> [l=132] XmlComment...verified.cs")]
    // public bool Star_7() =>
    //     Matcher.IsMatch(
    //         "XmlCommentDocumentationIdTests.CanMergeXmlCommentsWithDifferentDocumentationIdFormats#OpenApiXmlCommentSupport.generated.verified.cs",
    //         "*.generated.*",
    //         MatchFlags.Unix);
    //
    // [Benchmark(Description = "questionmark: ???????????????? -> password_manager")]
    // public bool QuestionMarks() =>
    //     Matcher.IsMatch(
    //         "password_generation",
    //         "????????????????",
    //         MatchFlags.Unix);
    //
    // [Benchmark(Description = "globstar: **/*.java -> [l=199,s=16] /chrome/browser/...ModuleTest.java")]
    // public bool Globstar_1() =>
    //     Matcher.IsMatch(
    //         "/chrome/browser/touch_to_fill/password_manager/password_generation/android/internal/java/src/org/chromium/chrome/browser/touch_to_fill/password_generation/TouchToFillPasswordGenerationModuleTest.java",
    //         "**/*.java",
    //         MatchFlags.Unix);
    //
    // [Benchmark(Description = "globstar: **/password*/**/*.java -> [l=199,s=16] /chrome/browser/...ModuleTest.java")]
    // public bool Globstar_2() =>
    //     Matcher.IsMatch(
    //         "/chrome/browser/touch_to_fill/password_manager/password_generation/android/internal/java/src/org/chromium/chrome/browser/touch_to_fill/password_generation/TouchToFillPasswordGenerationModuleTest.java",
    //         "**/password*/**/*.java",
    //         MatchFlags.Unix);
    //
    // [Benchmark(Description = "charclass: [aA]...[nN] -> application")]
    // public bool CharClass_1() =>
    //     Matcher.IsMatch(
    //         "application",
    //         "[aA][pP][pP][lL][iI][cC][aA][tT][iI][oO][nN]",
    //         MatchFlags.Unix);
    //
    // [Benchmark(Description = "charclass: [Aa]...[Nn] -> application")]
    // public bool CharClass_2() =>
    //     Matcher.IsMatch(
    //         "application",
    //         "[Aa][Pp][Pp][Ll][Ii][Cc][Aa][Tt][Ii][Oo][Nn]",
    //         MatchFlags.Unix);
    //
    // [Benchmark(Description = "charclass: [a-z]...[a-z] -> application")]
    // public bool CharClass_3() =>
    //     Matcher.IsMatch(
    //         "application",
    //         "[a-z][a-z][a-z][a-z][a-z][a-z][a-z][a-z][a-z][a-z][a-z]",
    //         MatchFlags.Unix);
    //
    // [Benchmark(Description = "charclass: [a-zA-Z0-9]...[a-zA-Z0-9] -> application")]
    // public bool CharClass_4() =>
    //     Matcher.IsMatch(
    //         "application",
    //         "[a-zA-Z0-9][a-zA-Z0-9][a-zA-Z0-9][a-zA-Z0-9][a-zA-Z0-9][a-zA-Z0-9][a-zA-Z0-9][a-zA-Z0-9][a-zA-Z0-9][a-zA-Z0-9][a-zA-Z0-9]",
    //         MatchFlags.Unix);
    //
    // [Benchmark(Description = "charclass: [0-9A-Za-z]...[0-9A-Za-z] -> application")]
    // public bool CharClass_5() =>
    //     Matcher.IsMatch(
    //         "application",
    //         "[0-9A-Za-z][0-9A-Za-z][0-9A-Za-z][0-9A-Za-z][0-9A-Za-z][0-9A-Za-z][0-9A-Za-z][0-9A-Za-z][0-9A-Za-z][0-9A-Za-z][0-9A-Za-z]",
    //         MatchFlags.Unix);
    //
    // [Benchmark(Description = "charclass: [a-zA-Z0-9]pplication -> application")]
    // public bool CharClass_6() =>
    //     Matcher.IsMatch(
    //         "application",
    //         "[a-zA-Z0-9]pplication",
    //         MatchFlags.Unix);
    //
    // [Benchmark(Description = "charclass: [0-9A-Za-z]pplication -> application")]
    // public bool CharClass_7() =>
    //     Matcher.IsMatch("application", "[0-9A-Za-z]pplication", MatchFlags.Unix);
    //
    // [Benchmark(Description = "pattern: *.jpg -> [l=54] sunset...1920x1080.jpg")]
    // public bool Pattern_1() =>
    //     Matcher.IsMatch(
    //         "sunset_over_mountain_lake_20260212153346_1920x1080.jpg",
    //         "*.jpg",
    //         MatchFlags.Unix);
    //
    // [Benchmark(Description = "pattern: *.cs -> [l=132] XmlComment...verified.cs")]
    // public bool Pattern_2() =>
    //     Matcher.IsMatch(
    //         "XmlCommentDocumentationIdTests.CanMergeXmlCommentsWithDifferentDocumentationIdFormats#OpenApiXmlCommentSupport.generated.verified.cs",
    //         "*.cs",
    //         MatchFlags.Unix);
    //
    // [Benchmark(Description = "brace: *.{jpg,png} -> [l=54] sunset...1920x1080.jpg")]
    // public bool Braces_1() =>
    //     Matcher.IsMatch(
    //         "sunset_over_mountain_lake_20260212153346_1920x1080.jpg",
    //         "*.{jpg,png}",
    //         MatchFlags.Unix);
    //
    // [Benchmark(Description = "brace: *.{jpg,png,gif,webp} -> [l=54] sunset...1920x1080.jpg")]
    // public bool Braces_2() =>
    //     Matcher.IsMatch("sunset_over_mountain_lake_20260212153346_1920x1080.webp",
    //     "*.{jpg,png,gif,webp}",
    //     MatchFlags.Unix);
    //
    // [Benchmark(Description = "path: 0/1/2/3 -> 0/1/2/3")]
    // public bool Path_1() =>
    //     Matcher.IsMatch("0/1/2/3", "0/1/2/3", MatchFlags.Unix);
    //
    // [Benchmark(Description = "path: 0000/1111/2222/3333/4444 -> 0000/.../4444")]
    // public bool Path_2() =>
    //     Matcher.IsMatch(
    //         "0000/1111/2222/3333/4444",
    //         "0000/1111/2222/3333/4444",
    //         MatchFlags.Unix);
    //
    // [Benchmark(Description = "path, star: */*/*/*/*/*/*/* -> 1/2/3/4/5/6/7/8")]
    // public bool Path_Star_1() =>
    //     Matcher.IsMatch("1/2/3/4/5/6/7/8", "*/*/*/*/*/*/*/*", MatchFlags.Unix);
    //
    // [Benchmark(Description =
    //     "path, star: [s=16] */*/.../*/* -> /chrome/.../TouchToFillPasswordGenerationModuleTest.java")]
    // public bool Path_Star_2() =>
    //     Matcher.IsMatch(
    //         "/chrome/browser/touch_to_fill/password_manager/password_generation/android/internal/java/src/org/chromium/chrome/browser/touch_to_fill/password_generation/TouchToFillPasswordGenerationModuleTest.java",
    //         "*/*/*/*/*/*/*/*/*/*/*/*/*/*/*/*",
    //         MatchFlags.Unix);
    //
    // [Benchmark(Description = "path, literal: [l=199] /chrome/...ModuleTest.java -> /chrome/...ModuleTest.java")]
    // public bool Path_Literal_1() =>
    //     Matcher.IsMatch(
    //         "/chrome/browser/touch_to_fill/password_manager/password_generation/android/internal/java/src/org/chromium/chrome/browser/touch_to_fill/password_generation/TouchToFillPasswordGenerationModuleTest.java",
    //         "/chrome/browser/touch_to_fill/password_manager/password_generation/android/internal/java/src/org/chromium/chrome/browser/touch_to_fill/password_generation/TouchToFillPasswordGenerationModuleTest.java",
    //         MatchFlags.Unix);
}

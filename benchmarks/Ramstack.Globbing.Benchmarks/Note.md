BenchmarkDotNet v0.15.8, Linux CachyOS
AMD Ryzen 9 5900X 1.73GHz, 1 CPU, 24 logical and 12 physical cores
.NET SDK 10.0.104
[Host]     : .NET 10.0.4 (10.0.4, 42.42.42.42424), X64 RyuJIT x86-64-v3
DefaultJob : .NET 10.0.4 (10.0.4, 42.42.42.42424), X64 RyuJIT x86-64-v3

| Group         | Pattern                         | Value                                           |    Mean |          Op/s |   IndexOf |         IndexOf |
|---------------|---------------------------------|-------------------------------------------------|--------:|--------------:|----------:|----------------:|
| literal       | a                               | a                                               |   7.026 | 142,321,725.3 |   ✔ 5.590 | ✔ 178,894,106.5 |
| literal       | src                             | src                                             |   9.506 | 105,195,155.3 |   ✔ 7.629 | ✔ 131,072,276.4 |
| literal       | source                          | source                                          |  11.963 |  83,593,772.6 |    11.347 |    88,130,073.7 |
| literal       | password_generation             | password_generation                             |  20.503 |  48,773,405.2 |    22.233 |    44,977,609.2 |
| star          | *                               | a                                               |   7.074 | 141,355,739.2 |   ✔ 4.764 | ✔ 209,926,476.5 |
| star          | *                               | source                                          |   7.046 | 141,926,616.7 |   ✔ 6.191 | ✔ 161,527,242.5 |
| star          | *                               | password_generation                             |   6.855 | 145,875,161.3 |   ✔ 6.225 | ✔ 160,634,650.2 |
| star          | *generation                     | password_generation                             |  38.473 |  25,992,511.4 |    38.518 |    25,962,122.2 |
| star          | password*                       | password_generation                             |  13.540 |  73,856,975.0 |    12.637 |    79,131,453.3 |
| star          | password*generation             | password_generation                             |  25.981 |  38,489,492.9 |    26.557 |    37,655,098.6 |
| star          | *.generated.*                   | [l=132] XmlComment...verified.cs                | 327.465 |   3,053,764.0 |   305.625 |     3,271,985.2 |
| questionmark  | ????????????????                | password_manager                                |  16.325 |  61,254,206.3 |    15.474 |    64,623,403.7 |
| globstar      | **/*.java                       | [l=199,s=16] /chrome/browser/...ModuleTest.java | 634.790 |   1,575,323.4 |   582.265 |     1,717,430.7 |
| globstar      | **/password*/**/*.java          | [l=199,s=16] /chrome/browser/...ModuleTest.java | 521.348 |   1,918,106.3 |   472.446 |     2,116,643.6 |
| charclass     | [aA]...[nN]                     | application                                     |  32.289 |  30,970,506.7 |    29.776 |    33,583,921.8 |
| charclass     | [Aa]...[Nn]                     | application                                     |  32.014 |  31,236,032.6 |    29.820 |    33,534,043.7 |
| charclass     | [a-z]...[a-z]                   | application                                     |  25.671 |  38,953,918.4 |    26.024 |    38,426,781.7 |
| charclass     | [a-zA-Z0-9]...[a-zA-Z0-9]       | application                                     |  48.751 |  20,512,333.4 |    52.198 |    19,157,910.1 |
| charclass     | [0-9A-Za-z]...[0-9A-Za-z]       | application                                     |  49.074 |  20,377,481.8 |    52.476 |    19,056,365.1 |
| charclass     | [a-zA-Z0-9]pplication           | application                                     |  18.960 |  52,742,644.8 |    17.231 |    58,034,423.0 |
| charclass     | [0-9A-Za-z]pplication           | application                                     |  19.015 |  52,589,136.4 |    18.513 |    54,017,011.0 |
| pattern       | *.jpg                           | [l=54] sunset...1920x1080.jpg                   | 137.728 |   7,260,668.7 |   136.722 |     7,314,120.9 |
| pattern       | *.cs                            | [l=132] XmlComment...verified.cs                | 367.438 |   2,721,548.9 |   341.344 |     2,929,598.9 |
| brace         | *.{jpg,png}                     | [l=54] sunset...1920x1080.jpg                   | 154.785 |   6,460,580.0 |   155.439 |     6,433,391.6 |
| brace         | *.{jpg,png,gif,webp}            | [l=54] sunset...1920x1080.jpg                   | 169.206 |   5,909,952.3 |   169.354 |     5,904,782.4 |
| path          | 0/1/2/3                         | 0/1/2/3                                         |  23.348 |  42,830,676.1 | ✔  17.064 | ✔  58,604,216.9 |
| path          | 0000/1111/2222/3333/4444        | 0000/1111/2222/3333/4444                        |  43.299 |  23,095,357.4 | ✔  32.740 | ✔  30,543,364.7 |
| path, star    | */*/*/*/*/*/*/*                 | 1/2/3/4/5/6/7/8                                 |  44.458 |  22,493,287.7 | ✔  29.100 | ✔  34,364,201.0 |
| path, star    | */*/*/*/*/*/*/*/*/*/*/*/*/*/*/* | [l=199,s=16] /chrome/browser/...ModuleTest.java |  97.207 |  10,287,286.8 | ✔  61.376 | ✔  16,293,119.6 |
| path, literal | /chrome/...ModuleTest.java      | [l=199,s=16] /chrome/browser/...ModuleTest.java | 249.692 |   4,004,931.1 | ✔ 214.788 | ✔   4,655,746.6 |

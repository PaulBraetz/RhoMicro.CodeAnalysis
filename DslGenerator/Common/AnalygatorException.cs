namespace RhoMicro.CodeAnalysis.DslGenerator.Common;
using System;

#if DSL_GENERATOR
[IncludeFile]
#endif
class AnalygatorException(String errorMessage) : Exception(errorMessage) { }
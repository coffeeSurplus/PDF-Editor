namespace PDF_Editor.Source.Data;

internal record FileError(string FilePath, string Message, string ErrorType, bool IsPasswordError = false);
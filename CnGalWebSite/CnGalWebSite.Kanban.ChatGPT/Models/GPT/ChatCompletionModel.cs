using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.ChatGPT.Models.GPT
{
    public class ChatCompletionModel
    {
        public string Model { get; set; } = "gpt-3.5-turbo";
        public List<ChatCompletionMessage> Messages { get; set; } = [];
        public List<ChatCompletionTool>? Tools { get; set; }
        public double temperature { get; set; } = 1.3;
    }

    public class ChatCompletionMessage
    {
        public string? Role { get; set; }
        public string? Content { get; set; }
        public List<ToolCall>? tool_calls { get; set; }
        public string? tool_call_id { get; set; }
    }

    public class ChatCompletionTool
    {
        public string Type { get; set; } = "function";
        public ChatCompletionFunction? Function { get; set; }
    }

    public class ChatCompletionFunction
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public ChatCompletionParameters? Parameters { get; set; }
    }

    public class ChatCompletionParameters
    {
        public string Type { get; set; } = "object";
        public Dictionary<string, ChatCompletionProperty>? Properties { get; set; }
        public List<string>? Required { get; set; }
    }

    public class ChatCompletionProperty
    {
        public string? Type { get; set; }
        public string? Description { get; set; }
    }

    public class ToolCall
    {
        public required string Id { get; set; }
        public string Type { get; set; } = "function";
        public required FunctionCall Function { get; set; }
    }

    public class FunctionCall
    {
        public required string Name { get; set; }
        public required string Arguments { get; set; }
    }

    public class ChatResult
    {
        public string? Id { get; set; }
        public string? Object { get; set; }
        public long Created { get; set; }
        public string? Model { get; set; }
        public List<Choice>? Choices { get; set; }
        public required Usage Usage { get; set; }
    }

    public class Choice
    {
        public int Index { get; set; }
        public ChatCompletionMessage? Message { get; set; }
        public string? Finish_reason { get; set; }
    }

    public class Usage
    {
        public int Prompt_tokens { get; set; }
        public int Completion_tokens { get; set; }
        public int Total_tokens { get; set; }
        public int prompt_cache_hit_tokens { get; set; }
        public int prompt_cache_miss_tokens { get; set; }
    }
}

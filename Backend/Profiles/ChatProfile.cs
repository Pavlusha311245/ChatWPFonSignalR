using AutoMapper;
using Server.ViewModel;

namespace Server.Profiles
{
    public class ChatProfile : Profile
    {
        public ChatProfile()
        {
            CreateMap<Models.Chat, ChatViewModel>();
        }
    }
}

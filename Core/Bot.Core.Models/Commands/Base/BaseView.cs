using Bot.Core.Models.Commands.Abstractions;

using TL;

using WTelegram;

namespace Bot.Core.Models.Commands.Base {
	public abstract class BaseView(Client client) : IView<UpdateNewMessage>, IView<UpdateBotCallbackQuery> {
		#region UpdateBotCallbackQuery

		public async Task ExecuteAsync(UpdateBotCallbackQuery update, User user) {
			var message = (Message)(await client.GetMessages(user, update.msg_id)).Messages[0];
			var @new = (await BuildMessage(update, user), await BuildMarkup(update, user));

			#region Set Default parameters

			if (string.IsNullOrEmpty(@new.Item1)) {
				@new.Item1 = await DefaultMessage(update, user);
			}

			@new.Item2 ??= await DefaultMarkup(update, user);

			#endregion
			
			if (@new.Item1.Length == 0 &&
				@new.Item2 == null ||
				message.message.Equals(@new.Item1) &&
				message.reply_markup.Equals(@new.Item2)
			   ) {
				return;
			}

			if (await IsValidAsync(update, user)) {
				await ProcessAsync(update, user);
			}
			else {
				await client.Messages_EditMessage(
					user,
					update.msg_id,
					@new.Item1 is {Length: > 0} ? @new.Item1 : null,
					reply_markup: @new.Item2 ?? message.reply_markup
				);
			}
		}

		public virtual Task<string> BuildMessage(UpdateBotCallbackQuery update, User user) {
			return Task.FromResult("");
		}

		public virtual Task<ReplyInlineMarkup?> BuildMarkup(UpdateBotCallbackQuery update, User user) {
			return Task.FromResult<ReplyInlineMarkup?>(null);
		}

		#endregion

		#region UpdateNewMessage

		public async Task ExecuteAsync(UpdateNewMessage update, User user) {
			if (await IsValidAsync(update, user)) {
				await ProcessAsync(update, user);
			}
			
			await client.Messages_SendMessage(
				user,
				await BuildMessage(update, user) is {Length: > 0} str ? str : await DefaultMessage((update.message as Message)!.message.Split(' '), user),
				Random.Shared.NextInt64(),
				reply_markup: await BuildMarkup(update, user) ?? await DefaultMarkup((update.message as Message)!.message.Split(' '), user)
			);
		}

		public virtual Task<string> BuildMessage(UpdateNewMessage update, User user) {
			return Task.FromResult("");
		}

		public virtual Task<ReplyInlineMarkup?> BuildMarkup(UpdateNewMessage update, User user) {
			return Task.FromResult<ReplyInlineMarkup?>(null);
		}

		#endregion

		protected virtual Task<string> DefaultMessage(object args, User user) => Task.FromResult("");

		protected virtual Task<ReplyInlineMarkup?> DefaultMarkup(object args, User user) => Task.FromResult<ReplyInlineMarkup?>(null);

		protected virtual Task<bool> IsValidAsync(object args, User user) => Task.FromResult(false);
		protected virtual Task ProcessAsync(object args, User user) => Task.CompletedTask;
	}
}
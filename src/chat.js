// EventSub channel.chat.message payloads contain the plain chat text here.
export function isLightsCommand(notification, broadcasterId) {
  return notification?.metadata?.message_type === 'notification' &&
    notification.metadata.subscription_type === 'channel.chat.message' &&
    notification.payload?.subscription?.type === 'channel.chat.message' &&
    notification.payload?.event?.broadcaster_user_id === broadcasterId &&
    notification.payload.event.message?.text?.trim() === '!lights';
}

export function chatMessage(notification, broadcasterId) {
  if (notification?.metadata?.message_type !== 'notification' ||
      notification.metadata.subscription_type !== 'channel.chat.message' ||
      notification.payload?.subscription?.type !== 'channel.chat.message' ||
      notification.payload?.event?.broadcaster_user_id !== broadcasterId) return null;
  const event = notification.payload.event;
  return { user: event.chatter_user_login || 'viewer', text: event.message?.text?.trim() || '' };
}

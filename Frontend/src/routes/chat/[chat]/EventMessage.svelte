<script lang="ts">
    import type { ChatItem } from "$lib/Handlers/chatHandler";
    export let item: ChatItem;
</script>

<p style="color: {item.content === 'UserPromoted' ? 'green' : item.content === 'UserDemoted' ? 'red' : item.content === 'OwnershipTransferred' ? 'yellow' : 'grey'}; font-style: italic;">
    {item.userName} has {item.content === 'UserJoined' ? 'joined' : item.content === 'UserLeft' ? 'left' : item.content === 'UserPromoted' ? 'been promoted in' : item.content === 'UserDemoted' ? 'been demoted in' : 'transferred ownership in'} the chat
</p>
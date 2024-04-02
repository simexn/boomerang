<script lang="ts">
    import type { ChatItem } from "$lib/Handlers/chatHandler";
    export let item: ChatItem;
</script>

<p style="color: {item.content === 'UserPromoted' ? 'green' : item.content === 'UserDemoted' ? 'red' : item.content === 'OwnershipTransferred' ? 'yellow' : 'grey'}; font-style: italic;">
    {item.userName} has 
    {#if item.content === 'UserJoined'}
        joined
    {:else if item.content === 'UserLeft'}
        left
    {:else if item.content === 'UserPromoted'}
        been promoted in
    {:else if item.content === 'UserDemoted'}
        been demoted in
    {:else if item.content === 'OwnershipTransferred'}
        transferred ownership in
    {:else}
        performed an action in
    {/if}
    the chat
</p>
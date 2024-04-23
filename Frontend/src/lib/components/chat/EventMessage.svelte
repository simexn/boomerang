<script lang="ts">
    import type { ChatItem } from "$lib/handlers/chatHandler";
    export let item: ChatItem;
</script>

<p style="color: {item.content === 'UserPromoted' ? 'green' : item.content === 'UserDemoted' ? `red` : item.content === 'OwnershipTransferred' ? '#3962b0c0' : 'grey'}; font-style: italic;">
    {item.userName}
    {#if item.content === 'UserJoined'}
        се присъедини към групата.
    {:else if item.content === 'UserLeft'}
        напусна групата.
    {:else if item.content === 'UserPromoted'}
        получи администраторски права в групата.
    {:else if item.content === 'UserDemoted'}
        загуби администраторските си права в групата.
    {:else if item.content === 'OwnershipTransferred'}
        стана собственик на групата.
    {:else if item.content === 'UserKicked'}
        беше изгонен от групата.
    {:else}
        {item.content}
        извърши действие в групата.
    {/if}
</p>
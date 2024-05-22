<script lang="ts">
    import type { ChatItem } from "$lib/handlers/chatHandler";
    export let item: ChatItem;
</script>

<p style="color: {item.content === 'UserPromoted' ? 'green' : item.content === 'UserDemoted' ? 'red' : item.content === 'OwnershipTransferred' ? '#3962b0c0' : item.content === 'UserBanned' ? "red" : 'grey'}; font-style: italic;">
    
    {#if item.content === 'UserJoined'}
        {item.userName} се присъедини към групата.
    {:else if item.content === 'UserLeft'}
        {item.userName} напусна групата.
    {:else if item.content === 'UserPromoted'}
        {item.userName} беше повишен в групата.
    {:else if item.content === 'UserDemoted'}
        {item.userName} беше понижен в групата.
    {:else if item.content === 'OwnershipTransferred'}
        {item.userName} прехвърли собствеността на групата.
    {:else if item.content === 'UserBanned'}
        {item.userName} беше изгонен от групата.
    {:else}
        {item.userName} извърши действие в групата.
    {/if}
</p>
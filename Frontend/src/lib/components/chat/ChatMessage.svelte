<script lang="ts">
    import type { User } from "$lib/handlers/accountHandler";
    import type {ChatItem} from "$lib/handlers/chatHandler";
    import { writable, type Stores } from "svelte/store";
    import { handleEditMessage, handleDeleteMessage } from "$lib/handlers/chatHandler";
    import EventMessage from "./EventMessage.svelte";
    import UserMessage from "./UserMessage.svelte";
    import '$lib/css/message.css';
    import { onMount } from "svelte";
    import type { Group } from "$lib/handlers/groupHandler";
    export let imageUrl: string;
    export let userInfo: User;
    export let scrollContainer: HTMLElement;
    export let chatItems:ChatItem[];
    export let chatId: string;
    export let groupInfo: Group;
    export let handleScroll: any;
    const isEditing = writable();
    let originalMessage = '';
    onMount(() => {
        scrollContainer.scrollTop = scrollContainer.scrollHeight;
    });

    function isEditingMessage(id: number, content:string) {
        originalMessage = content;
        isEditing.set(id);
    }

    async function confirmEdit(id:number, newContent: string) {
        
        await handleEditMessage(chatId, id, newContent);
        isEditing.set(null);
    }

    async function cancelEdit(id: number) {
        isEditing.set(null);
        chatItems.map(item => item.id === id ? item.content = originalMessage : item);
    }
    
    async function deleteMessage(id: number) {
        await handleDeleteMessage(chatId, id);
    } 
</script>

<div class="messages-container" bind:this={scrollContainer} on:scroll={handleScroll}>
    
    {#each chatItems as item (`${item.id}-${item.isEvent ? 'event' : 'message'}`)}
    <div class="message" style="display: flex; align-items: start;">
        {#if item.isEvent}
            <EventMessage {item} />
        {:else}
            <UserMessage {groupInfo} {item} {imageUrl} {userInfo} {isEditingMessage} {deleteMessage} {confirmEdit} {cancelEdit} {isEditing} withoutUserDetails={false} />
        {/if}
    </div>
    {/each}
</div>

<style>
.messages-container{
    display: flex;
    flex-direction: column;
    word-wrap: break-word;
    padding: 0 0 0 5px;
    height: 100% !important;
    min-height: 0 !important;
    max-height: 100% !important;
    box-sizing: border-box;
    overflow: auto;
    z-index: 400;
}
.message {
    transition: background-color 100ms;
    position:relative;
    padding-left: 1rem;
}

.message:hover {
    background-color: #f5f5f5;
}


</style>
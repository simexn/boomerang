<script lang="ts">
    import type { User } from "$lib/Handlers/accountHandler";
    import type {ChatItem} from "$lib/Handlers/chatHandler";
    import { writable, type Stores } from "svelte/store";
    import { handleEditMessage, handleDeleteMessage } from "$lib/Handlers/chatHandler";
    import EventMessage from "./EventMessage.svelte";
    import UserMessage from "./UserMessage.svelte";
    import '$lib/css/message.css';
    export let imageUrl: string;
    export let userInfo: User;
    export let scrollContainer;
    export let chatItems:ChatItem[];
    export let chatId: string;
    const isEditing = writable();

    let originalMessage = '';

    
    function isEditingMessage(id: number, content:string) {
        originalMessage = content;
        isEditing.set(id);
    }

    async function confirmEdit(id:number, newContent: string) {
        
        await handleEditMessage(chatId, id, newContent);
        isEditing.set(null);
    }

    
    async function deleteMessage(id: number) {
        await handleDeleteMessage(chatId, id);
    }

    
    
</script>
<div class="messages-container" style="height: 47rem; overflow-y: auto;" bind:this={scrollContainer}>
    {#each chatItems as item}
        <div class="message" style="display: flex; align-items: start;">
            {#if item.isEvent}
                <EventMessage {item} />
            {:else}
                <UserMessage {item} {imageUrl} {userInfo} {isEditingMessage} {deleteMessage} {confirmEdit} {isEditing} />
            {/if}
        </div>
    {/each}
</div>

<style>
.messages-container{
    
    padding: 8px 0.5em 0 1.5em;
    word-wrap: break-word;
    padding: 0 0 0 5px;
    margin: 0 auto;
    overflow: auto;
    height: 100%;

}
.message {
    transition: background-color 100ms;
    position:relative;
    flex-grow: 1;
    padding-left: 1rem;
}

.message:hover {
    background-color: #f5f5f5;
}


</style>
<script lang="ts">
    import { fade, fly } from "svelte/transition";
    import {handleAddFriend} from "$lib/Handlers/userHandler";
    let modalBody: any;
    let friendUsername: string;
    let isFriendUsernameValid = true;

    export let friendsModalActive: boolean;
    export let directChats: any =[];

    async function addFriend(event: Event){
        event.preventDefault();
        if(friendUsername.length < 4 || friendUsername.length > 20){
            isFriendUsernameValid = false;
            return;
        }
        isFriendUsernameValid = true;
        directChats = await handleAddFriend(friendUsername);
        friendUsername = '';
        friendsModalActive = false;
    }
</script>

<div class="modal" transition:fade={{duration: 200}}>
    <div class="modal-dialog modal-dialog-centered modal-lg">
        <div class="modal-content">
            <div class="modal-header p-0">
                <div class="d-flex flex-row w-100" style="justify-content:center;">
                    <h5 class="modal-title w-100 h-100 pt-1 pb-1 text-center"><b>Add a friend</b></h5>
                </div>
            </div>
            <div class="modal-body" bind:this={modalBody}>
                <form on:submit|preventDefault={addFriend} in:fly={{x: 200, duration: 500}} on:introstart={() => modalBody.style.overflowY = 'hidden'} on:outroend={() => modalBody.style.overflowY = 'auto'}>
                    <div class="mb-3">
                        <label for="inviteCodeJoin" class="form-label">Username:</label>
                        <input type="text" maxlength="20" bind:value={friendUsername} class="form-control" id="inviteCodeJoin" class:invalid-input={!isFriendUsernameValid}>
                        {#if !isFriendUsernameValid}
                            <div class="invalid-feedback">
                                Invalid user.
                            </div>
                        {/if}
                    </div>
                </form>
            </div>
            <div class="modal-footer">
                <button type="submit" class="btn btn-primary" on:click|preventDefault={addFriend}>Submit</button>
                <button type="button" class="btn btn-secondary" on:click={() => friendsModalActive = false}>Close</button>
            </div>
        </div>
    </div>
</div>

<style>
    .modal {                                              
            background-color: rgba(0, 0, 0, 0.5);
            display: flex;
            justify-content: center;
            align-items: center;
            z-index: 1000;
        }
            .modal-content{
                width: 20rem;
            }

            h5.modal-title{
                border-radius: 0!important;
            }
            .modal-body {
                height: 300px;
                overflow:hidden;
                overflow-x:hidden; /* Adjust this value as needed */
                overflow-y: auto; /* Enable scrolling if the content is taller than the modal body */
            }
            .invalid-input{
                border: 1px solid red;
            }
            .invalid-feedback{
                display: block;
            }
            .btn.active {
                    background-color: blue; /* Change this to your preferred color */
                    color: white; /* Change this to your preferred color */
                }
</style>